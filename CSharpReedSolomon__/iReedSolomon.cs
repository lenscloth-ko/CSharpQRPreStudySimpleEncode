using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpReedSolomon
{
    /// <summary>
    /// Reed Solomon 부호화 구현 클래스
    /// </summary>
    public class iReedSolomon
    {
        //public static object[] pPP = new object[16 + 1];
        public static byte[][] pPP = new byte[16 + 1][];

        public const int DEFAULT_MM = 8;
        public const int DEFAULT_NP = 32;

        // 갈루아 체 원시 다항식
        public static byte[] PP2 = { 1, 1, 1 };
        public static byte[] PP3 = { 1, 1, 0, 1 };        // 1 + x + x^3
        public static byte[] PP4 = { 1, 1, 0, 0, 1 };        // 1 + x + x^4
        public static byte[] PP5 = { 1, 0, 1, 0, 0, 1 };        // 1 + x^2 + x^5
        public static byte[] PP6 = { 1, 1, 0, 0, 0, 0, 1 };        // 1 + x + x^6
        public static byte[] PP7 = { 1, 0, 0, 1, 0, 0, 0, 1 };        // 1 + x^3 + x^7
        public static byte[] PP8 = { 1, 0, 1, 1, 1, 0, 0, 0, 1 };        // 1+x^2+x^3+x^4+x^8
        public static byte[] PP9 = { 1, 0, 0, 0, 1, 0, 0, 0, 0, 1 };        // 1+x^4+x^9
        public static byte[] PP10 = { 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 };        // 1+x^3+x^10
        public static byte[] PP11 = { 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 };        // 1+x^2+x^11
        public static byte[] PP12 = { 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 };        // 1+x+x^4+x^6+x^12
        public static byte[] PP13 = { 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 };        // 1+x+x^3+x^4+x^13
        public static byte[] PP14 = { 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 };        // 1+x+x^6+x^10+x^14
        public static byte[] PP15 = { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };        // 1+x+x^15
        public static byte[] PP16 = { 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1 };        // 1+x+x^3+x^12+x^16


        private int FGFDimension = 0;
        private int FRSLength = 0;
        private int FDataLength = 0;
        private int FParityLength = 0;
        private int FMaxRSLength = 0;
        private byte[] PP = new byte[16 + 1];

        // specify irreducible polynomial coeffts
        // 갈루아 체 원시 다항식
        private short[] ExpToVector;
        // alpha to
        private short[] VectorToExp;
        // index of
        private short[] gg;
        // 오류 정정 생성 다항식
        private short[] bb;
        private short[] data;
        private short[] recd;
        private byte[] CodeWord;


        public int DataLength
        {
            get
            {
                return FDataLength;
            }
        }

        public iReedSolomon()
        {
            pPP[2] = PP2;
            pPP[3] = PP3;
            pPP[4] = PP4;
            pPP[5] = PP5;
            pPP[6] = PP6;
            pPP[7] = PP7;
            pPP[8] = PP8;
            pPP[9] = PP9;
            pPP[10] = PP10;
            pPP[11] = PP11;
            pPP[12] = PP12;
            pPP[13] = PP13;
            pPP[14] = PP14;
            pPP[15] = PP15;
            pPP[16] = PP16;

            InitBuffers();
            // Clear the internal buffers
            // generate the Galois Field GF(2**mm)
            // Generate_gf(DEFAULT_MM);
            // FGFDimension:=DEFAULT_MM;
            InitParameters(DEFAULT_MM, DEFAULT_NP);
        }

        public iReedSolomon(int GFDimension, int RSLength, int ParityLength)
        {
            pPP[2] = PP2;
            pPP[3] = PP3;
            pPP[4] = PP4;
            pPP[5] = PP5;
            pPP[6] = PP6;
            pPP[7] = PP7;
            pPP[8] = PP8;
            pPP[9] = PP9;
            pPP[10] = PP10;
            pPP[11] = PP11;
            pPP[12] = PP12;
            pPP[13] = PP13;
            pPP[14] = PP14;
            pPP[15] = PP15;
            pPP[16] = PP16;

            InitBuffers();
            InitParameters(DEFAULT_MM, DEFAULT_NP);

            SetGFDimension(GFDimension);
            SetRSLength(RSLength, ParityLength);

        }

        public void dispose()
        {
        }

        private void InitParameters(int GFDimension, int ParityLength)
        {
            FGFDimension = GFDimension;
            FParityLength = ParityLength;
            FMaxRSLength = (1 << FGFDimension) - 1; // nn=2**mm -1   Max length of codeword
            FRSLength = (1 << FGFDimension) - 1;    // nn=2**mm -1   Max length of codeword
            FDataLength = FRSLength - FParityLength;// data symbols, kk = nn-2*MaxErrors

            gg = new short[FParityLength + 1];
            bb = new short[FParityLength - 1 + 1];
            data = new short[FDataLength + 1];

            ExpToVector = new short[FRSLength + 1];
            VectorToExp = new short[FRSLength + 1];
            CodeWord = new byte[FRSLength - 1 + 1];
            recd = new short[FRSLength - 1 + 1];

            Generate_GF(FGFDimension);
            gen_poly();
        }

        private void ReInitParameters(int GFDimension, int ParityLength, int RSLength)
        {
            FGFDimension = GFDimension;
            FParityLength = ParityLength;
            FMaxRSLength = (1 << FGFDimension) - 1;        // nn=2**mm -1   Max length of codeword
            FRSLength = RSLength;
            FDataLength = FRSLength - FParityLength;       // data symbols, kk = nn-2*MaxErrors

            gg = new short[FParityLength + 1];
            bb = new short[FParityLength - 1 + 1];
            data = new short[FDataLength + 1];

            ExpToVector = new short[FMaxRSLength + 1];
            VectorToExp = new short[FMaxRSLength + 1];
            CodeWord = new byte[FRSLength - 1 + 1];
            recd = new short[FRSLength - 1 + 1];

            Generate_GF(FGFDimension);
            gen_poly();
        }

        protected void Generate_GF(int mm)
        {
            int i;
            short mask;
            SetPrimitive(ref PP, mm);
            mask = 1;
            ExpToVector[mm] = 0;
            for (i = 0; i <= (mm - 1); i++)
            {
                ExpToVector[i] = mask;
                VectorToExp[ExpToVector[i]] = (short)i;
                if (PP[i] != 0)
                {
                    ExpToVector[mm] = (short)(ExpToVector[mm] ^ mask);
                }
                mask = (short)(mask << 1);
            }
            VectorToExp[ExpToVector[mm]] = (short)mm;
            mask = (short)(mask >> 1);
            for (i = (mm + 1); i <= (FRSLength - 1); i++)
            {
                if (ExpToVector[i - 1] >= mask)
                {
                    ExpToVector[i] = (short)(ExpToVector[mm] ^ ((ExpToVector[i - 1] ^ mask) << 1));
                }
                else
                {
                    ExpToVector[i] = (short)(ExpToVector[i - 1] << 1);
                }
                VectorToExp[ExpToVector[i]] = (short)i;
            }
            VectorToExp[0] = -1;
        }

        protected void gen_poly()
        {
            short i;
            short j;
            gg[0] = 2;
            // primitive element alpha = 2  for GF(2**mm)
            gg[1] = 1;
            // g(x) = (X+alpha) initially
            for (i = 2; i <= (FRSLength - FDataLength); i++)
            {
                gg[i] = 1;
                for (j = (short)(i - 1); j >= 1; j--)
                {
                    if ((gg[j] != 0))
                    {
                        gg[j] = (short)(gg[j - 1] ^ ExpToVector[(VectorToExp[gg[j]] + i) % FMaxRSLength]);
                    }
                    else
                    {
                        gg[j] = gg[j - 1];
                    }
                }
                // gg[0] can never be zero
                gg[0] = ExpToVector[(VectorToExp[gg[0]] + i) % FMaxRSLength];
            }
            // Convert gg[] to index form for quicker encoding.
            for (i = 0; i <= FParityLength; i++)
            {
                gg[i] = VectorToExp[gg[i]];
            }
        }

        protected void SetPrimitive(ref byte[] PP, int nIdx)
        {
            //@ Unsupported function or procedure: 'Move'
            // Move(Units.iPentecReedSolomon.pPP[nIdx], PP, (nIdx + 1));
            //Array.Copy(pPP, nIdx, PP, 0, nIdx+1);
            //Array.Copy(pPP[nIdx], 0, PP, 0, nIdx+1);
            for (int i = 0; i < nIdx + 1; i++)
            {
                PP[i] = pPP[nIdx][i];
            }
        }

        // TReedSolomon.InitBuffers
        public void InitBuffers()
        {
            /*
              //@ Unsupported function or procedure: 'FillChar'
              FillChar(data, sizeof(data), 0);
              //@ Unsupported function or procedure: 'FillChar'
              FillChar(recd, sizeof(recd), 0);
              //@ Unsupported function or procedure: 'FillChar'
              FillChar(CodeWord, sizeof(CodeWord), 0);
             */
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0;
                }
            }
            if (recd != null)
            {
                for (int i = 0; i < recd.Length; i++)
                {
                    recd[i] = 0;
                }
            }
            if (CodeWord != null)
            {
                for (int i = 0; i < CodeWord.Length; i++)
                {
                    CodeWord[i] = 0;
                }
            }
        }

        public void EncodeRS(ref byte[] xData, ref byte[] xEncoded)
        {
            //int e;
            byte[] rswork = new byte[123 - 1 + 1];
            // RS 부호 계산 용 작업 영역
            int nI;
            int i;
            int j;
            short feedback;
            //byte[] axData;

            for (i = 0; i < bb.Length; i++)
            {
                bb[i] = 0;
            }
            for (nI = 0; nI <= (FDataLength - 1); nI++)
            {
                //data[nI] = axData[nI];
                data[nI] = xData[nI];
            }
            for (i = (FDataLength - 1); i >= 0; i--)
            {
                feedback = VectorToExp[data[i] ^ bb[FParityLength - 1]];
                if ((feedback != -1))
                {
                    for (j = (FParityLength - 1); j >= 1; j--)
                    {
                        if ((gg[j] != -1))
                        {
                            bb[j] = (short)(bb[j - 1] ^ ExpToVector[(gg[j] + feedback) % FMaxRSLength]);
                        }
                        else
                        {
                            bb[j] = bb[j - 1];
                        }
                    }
                    bb[0] = ExpToVector[(gg[0] + feedback) % FMaxRSLength];
                }
                else
                {
                    for (j = (FParityLength - 1); j >= 1; j--)
                    {
                        bb[j] = bb[j - 1];
                    }
                    bb[0] = 0;
                }
            }
            // put the transmitted codeword, made up of data
            // plus parity, in CodeWord
            for (nI = 0; nI <= (FParityLength - 1); nI++)
            {
                recd[nI] = bb[nI];
            }
            for (nI = 0; nI <= (FDataLength - 1); nI++)
            {
                recd[nI + FParityLength] = data[nI];
            }
            for (nI = 0; nI <= (FRSLength - 1); nI++)
            {
                CodeWord[nI] = (byte)recd[nI];
            }

            //@ Unsupported function or procedure: 'Move'
            //Move(CodeWord[0], xEncoded, FRSLength);
            Array.Copy(CodeWord, 0, xEncoded, 0, FRSLength);
        }

        public void EncodeRSEx(ref byte[] xData, ref byte[] xEncoded, ref byte[] xParity)
        {
            int nI;
            int i;
            int j;
            short feedback;
            int e;
            short[] invertgg;
            invertgg = new short[gg.Length];
            for (i = 0; i < gg.Length; i++)
            {
                //invertgg[i] = gg[gg.Length - 1 - 1 - i];
                invertgg[i] = gg[gg.Length - 1 - i];
            }
            for (i = 0; i < bb.Length; i++)
            {
                bb[i] = 0;
            }
            //for (nI = 0; nI <= (FRSLength - 1); nI++) {

            for (nI = 0; nI <= (FDataLength - 1); nI++)
            {
                data[nI] = xData[nI];
            }
            for (i = (FDataLength - 1); i >= 0; i--)
            {
                feedback = VectorToExp[data[i] ^ bb[FParityLength - 1]];
                if ((feedback != -1))
                {
                    for (j = (FParityLength - 1); j >= 1; j--)
                    {
                        if ((gg[j] != -1))
                        {
                            bb[j] = (short)(bb[j - 1] ^ ExpToVector[(gg[j] + feedback) % FMaxRSLength]);
                        }
                        else
                        {
                            bb[j] = bb[j - 1];
                        }
                    }
                    bb[0] = ExpToVector[(gg[0] + feedback) % FMaxRSLength];
                }
                else
                {
                    for (j = (FParityLength - 1); j >= 1; j--)
                    {
                        bb[j] = bb[j - 1];
                    }
                    bb[0] = 0;
                }
            }
            // put the transmitted codeword, made up of data
            // plus parity, in CodeWord
            for (nI = 0; nI <= (FParityLength - 1); nI++)
            {
                recd[nI] = bb[nI];
                xParity[nI] = (byte)bb[nI];
            }
            for (nI = 0; nI <= (FDataLength - 1); nI++)
            {
                recd[nI + FParityLength] = data[nI];
            }
            for (nI = 0; nI <= (FRSLength - 1); nI++)
            {
                CodeWord[nI] = (byte)recd[nI];
            }
            //@ Unsupported function or procedure: 'Move'
            //Move(CodeWord[0], xEncoded, FRSLength);
            Array.Copy(CodeWord, 0, xEncoded, 0, FRSLength);
        }

        //public int DecodeRS(ref object xData, ref object xDecoded)
        public int DecodeRS(ref byte[] xData, ref byte[] xDecoded)
        {
            int result;
            //byte[] axData;
            //byte[] axDecoded;
            //string cStr;
            int nI;
            int nJ;
            int nK;
            int i;
            int j;
            short u;
            short q;
            short[][] elp;            // Array[0..np+1 , 0..np - 1] of smallint ;
            short[] d;            // Array[0..np+1] of smallint ;
            short[] l;            // Array[0..np+1] of smallint ;
            short[] u_lu;            // Array[0..np+1] of smallint ;
            short[] s;            // Array[0..np]   of smallint ;
            short count;
            short syn_error;
            int lpc;
            short[] root;            // Array[0..MaxErrors - 1] of smallint ;
            short[] loc;            // Array[0..MaxErrors - 1] of smallint ;
            short[] z;            // Array[0..MaxErrors]     of smallint ;
            short[] err;            // Array[0..nn-1]          of smallint ;
            short[] reg;            // Array[0..MaxErrors]     of smallint ;
                                    // DecodeRS

            elp = new short[FParityLength + 1 + 1][];
            for (lpc = 0; lpc < elp.Length; lpc++)
            {
                //@ Unsupported function or procedure: 'SetLength'
                //elp[lpc].Length = FParityLength - 1 + 1;
                elp[lpc] = new short[FParityLength - 1 + 1];
            }

            d = new short[FParityLength + 1 + 1];
            l = new short[FParityLength + 1 + 1];
            u_lu = new short[FParityLength + 1 + 1];
            s = new short[FParityLength + 1];
            root = new short[Convert.ToInt64(FParityLength / 2) - 1 + 1];
            loc = new short[Convert.ToInt64(FParityLength / 2) - 1 + 1];
            z = new short[Convert.ToInt64(FParityLength / 2) + 1];
            err = new short[FRSLength - 1 + 1];
            reg = new short[Convert.ToInt64(FParityLength / 2) + 1];
            for (nI = 0; nI <= (FRSLength - 1); nI++)
            {
                //recd[nI] = axData[nI];
                recd[nI] = xData[nI];
            }
            for (i = 0; i <= (FRSLength - 1); i++)
            {
                recd[i] = VectorToExp[recd[i]];
            }
            // put recd[i] into index form
            count = 0;
            syn_error = 0;
            result = 0;
            // first form the syndromes
            for (i = 1; i <= FParityLength; i++)
            {
                s[i] = 0;
                for (j = 0; j <= (FRSLength - 1); j++)
                {
                    if (recd[j] != -1)
                    {
                        // recd[j] in index form
                        s[i] = (short)(s[i] ^ ExpToVector[(recd[j] + i * j) % FMaxRSLength]);
                    }
                }
                // convert syndrome from polynomial form to index form
                if ((s[i] != 0))
                {
                    syn_error = 1;
                    // set flag if non-zero syndrome => error
                }
                s[i] = VectorToExp[s[i]];
            }
            if (syn_error != 0)
            {
                d[0] = 0;
                // index form
                d[1] = s[1];
                // index form
                elp[0][0] = 0;
                // index form
                elp[1][0] = 1;
                // polynomial form
                for (i = 1; i <= (FParityLength - 1); i++)
                {
                    elp[0][i] = -1;
                    // index form
                    elp[1][i] = 0;
                    // polynomial form
                }
                l[0] = 0;
                l[1] = 0;
                u_lu[0] = -1;
                u_lu[1] = 0;
                u = 0;
                while (((u < FParityLength) && (l[u + 1] <= Convert.ToInt64(FParityLength / 2))))
                {
                    u++;
                    if ((d[u] == -1))
                    {
                        l[u + 1] = l[u];
                        for (i = 0; i <= l[u]; i++)
                        {
                            elp[u + 1][i] = elp[u][i];
                            elp[u][i] = VectorToExp[elp[u][i]];
                        }
                    }
                    else
                    {
                        // search for words with greatest u_lu[q] for which d[q]!=0
                        q = (short)(u - 1);
                        while (((d[q] == -1) && (q > 0)))
                        {
                            q -= 1;
                        }
                        // have found first non-zero d[q]
                        if ((q > 0))
                        {
                            j = q;
                            while (j > 0)
                            {
                                j -= 1;
                                if (((d[j] != -1) && (u_lu[q] < u_lu[j])))
                                {
                                    q = (short)j;
                                }
                            }
                        }
                        // have now found q such that d[u]!=0 and u_lu[q] is maximum
                        // store degree of new elp polynomial
                        if ((l[u] > l[q] + u - q))
                        {
                            l[u + 1] = l[u];
                        }
                        else
                        {
                            l[u + 1] = (short)(l[q] + u - q);
                        }
                        // form new elp(x)
                        for (i = 0; i <= (FParityLength - 1); i++)
                        {
                            elp[u + 1][i] = 0;
                        }
                        for (i = 0; i <= l[q]; i++)
                        {
                            // if (elp[q][i] <> -1) then elp[u+1][i+u-q] := ExpToVector[(d[u] + FRSLength - d[q] + elp[q][i]) mod FMaxRSLength] ;
                            if ((elp[q][i] != -1))
                            {
                                elp[u + 1][i + u - q] = ExpToVector[(d[u] + FMaxRSLength - d[q] + elp[q][i]) % FMaxRSLength];
                            }
                        }
                        for (i = 0; i <= l[u]; i++)
                        {
                            elp[u + 1][i] = (short)(elp[u + 1][i] ^ elp[u][i]);
                            // convert old elp value to index
                            elp[u][i] = VectorToExp[elp[u][i]];
                        }
                    }
                    u_lu[u + 1] = (short)(u - l[u + 1]);
                    // form (u+1)th discrepancy
                    if (u < FParityLength)
                    {
                        // no discrepancy computed on last iteration
                        if ((s[u + 1] != -1))
                        {
                            d[u + 1] = ExpToVector[s[u + 1]];
                        }
                        else
                        {
                            d[u + 1] = 0;
                        }
                        for (i = 1; i <= l[u + 1]; i++)
                        {
                            if (((s[u + 1 - i] != -1) && (elp[u + 1][i] != 0)))
                            {
                                d[u + 1] = (short)(d[u + 1] ^ ExpToVector[(s[u + 1 - i] + VectorToExp[elp[u + 1][i]]) % FMaxRSLength]);
                            }
                        }
                        // put d[u+1] into index form
                        d[u + 1] = VectorToExp[d[u + 1]];
                    }
                }
                // end While
                u++;
                if (l[u] <= Convert.ToInt64(FParityLength / 2))
                {
                    // can correct error
                    // put elp into index form
                    for (i = 0; i <= l[u]; i++)
                    {
                        elp[u][i] = VectorToExp[elp[u][i]];
                    }
                    // find roots of the error location polynomial
                    for (i = 1; i <= l[u]; i++)
                    {
                        reg[i] = elp[u][i];
                    }
                    // for i := 1 to FRSLength do begin
                    for (i = 1; i <= FMaxRSLength; i++)
                    {
                        q = 1;
                        for (j = 1; j <= l[u]; j++)
                        {
                            if (reg[j] != -1)
                            {
                                reg[j] = (short)((reg[j] + j) % FMaxRSLength);
                                q = (short)(q ^ ExpToVector[reg[j]]);
                            }
                        }
                        if (q == 0)
                        {
                            // store root and error location number indices
                            root[count] = (short)i;
                            // loc[count] := FRSLength - i ;
                            loc[count] = (short)(FMaxRSLength - i);
                            count++;
                        }
                    }
                    if (count == l[u])
                    {
                        // no. roots = degree of elp hence <= tt errors
                        result = count;
                        // form polynomial z(x)
                        for (i = 1; i <= l[u]; i++)
                        {
                            // Z[0] = 1 always - do not need
                            if (((s[i] != -1) && (elp[u][i] != -1)))
                            {
                                z[i] = (short)(ExpToVector[s[i]] ^ ExpToVector[elp[u][i]]);
                            }
                            else if (((s[i] != -1) && (elp[u][i] == -1)))
                            {
                                z[i] = ExpToVector[s[i]];
                            }
                            else if (((s[i] == -1) && (elp[u][i] != -1)))
                            {
                                z[i] = ExpToVector[elp[u][i]];
                            }
                            else
                            {
                                z[i] = 0;
                            }
                            for (j = 1; j <= (i - 1); j++)
                            {
                                if (((s[j] != -1) && (elp[u][i - j] != -1)))
                                {
                                    z[i] = (short)(z[i] ^ ExpToVector[(elp[u][i - j] + s[j]) % FMaxRSLength]);
                                }
                            }
                            // put into index form
                            z[i] = VectorToExp[z[i]];
                        }
                        // evaluate errors at locations given by
                        // error location numbers loc[i]
                        for (i = 0; i <= (FRSLength - 1); i++)
                        {
                            // for i := 0 to (FMaxRSLength - 1) do begin
                            err[i] = 0;
                            // convert recd[] to polynomial form
                            if (recd[i] != -1)
                            {
                                recd[i] = ExpToVector[recd[i]];
                            }
                            else
                            {
                                recd[i] = 0;
                            }
                        }
                        for (i = 0; i <= (l[u] - 1); i++)
                        {
                            // compute numerator of error term first
                            err[loc[i]] = 1;
                            // accounts for z[0]
                            for (j = 1; j <= l[u]; j++)
                            {
                                if (z[j] != -1)
                                {
                                    err[loc[i]] = (short)(err[loc[i]] ^ ExpToVector[(z[j] + j * root[i]) % FMaxRSLength]);
                                }
                            }
                            if (err[loc[i]] != 0)
                            {
                                err[loc[i]] = VectorToExp[err[loc[i]]];
                                q = 0;
                                // form denominator of error term
                                for (j = 0; j <= (l[u] - 1); j++)
                                {
                                    if (j != i)
                                    {
                                        q = (short)(q + VectorToExp[1 ^ ExpToVector[(loc[j] + root[i]) % FMaxRSLength]]);
                                    }
                                }
                                q = (short)(q % FMaxRSLength);
                                // err[loc[i]] := ExpToVector[(err[loc[i]] - q + FRSLength) mod FMaxRSLength] ;
                                err[loc[i]] = ExpToVector[(err[loc[i]] - q + FMaxRSLength) % FMaxRSLength];
                                // recd[i] must be in polynomial form
                                recd[loc[i]] = (short)(recd[loc[i]] ^ err[loc[i]]);
                            }
                        }
                    }
                    else
                    {
                        // no. roots != degree of elp => >tt errors and cannot solve
                        result = -1;
                        // Signal an error.
                        for (i = 0; i <= (FRSLength - 1); i++)
                        {
                            // could return error flag if desired
                            if (recd[i] != -1)
                            {
                                // convert recd[] to polynomial form
                                recd[i] = ExpToVector[recd[i]];
                            }
                            else
                            {
                                recd[i] = 0;
                            }
                            // just output received codeword as is
                        }
                    }
                }
                else
                {
                    // if l[u] <= tt then
                    // elp has degree has degree >tt hence cannot solve
                    for (i = 0; i <= (FRSLength - 1); i++)
                    {
                        // could return error flag if desired
                        if (recd[i] != -1)
                        {
                            // convert recd[] to polynomial form
                            recd[i] = ExpToVector[recd[i]];
                        }
                        else
                        {
                            recd[i] = 0;
                        }
                        // just output received codeword as is
                    }
                }
            }
            else
            {
                // If syn_error <> 0 then
                // no non-zero syndromes => no errors: output received codeword
                for (i = 0; i <= (FRSLength - 1); i++)
                {
                    if (recd[i] != -1)
                    {
                        // convert recd[] to polynomial form
                        recd[i] = ExpToVector[recd[i]];
                    }
                    else
                    {
                        recd[i] = 0;
                    }
                    result = 0;
                    // No errors ocurred.
                }
            }

            //xDecoded = new byte[FRSLength - 1];
            for (nI = 0; nI <= (FRSLength - 1); nI++)
            {
                xDecoded[nI] = (byte)recd[nI];
            }
            return result;
        }

        public int DecodeRSEx(ref byte[] xData, ref byte[] xParity, ref byte[] xDecoded)
        {
            int result;
            string cStr;
            int nI;
            int nJ;
            int nK;
            int i;
            int j;
            short u;
            short q;
            short[][] elp;        // Array[0..np+1 , 0..np - 1] of smallint ;
            short[] d;            // Array[0..np+1] of smallint ;
            short[] l;            // Array[0..np+1] of smallint ;
            short[] u_lu;         // Array[0..np+1] of smallint ;
            short[] s;            // Array[0..np]   of smallint ;
            short count;
            short syn_error;
            int lpc;
            short[] root;         // Array[0..MaxErrors - 1] of smallint ;
            short[] loc;          // Array[0..MaxErrors - 1] of smallint ;
            short[] z;            // Array[0..MaxErrors]     of smallint ;
            short[] err;          // Array[0..nn-1]          of smallint ;
            short[] reg;          // Array[0..MaxErrors]     of smallint ;

            // DecodeRS
            elp = new short[FParityLength + 1 + 1][];
            for (lpc = 0; lpc < elp.Length; lpc++)
            {
                elp[lpc] = new short[FParityLength - 1 + 1];
                //@ Unsupported function or procedure: 'SetLength'
                //elp[lpc].Length = FParityLength - 1 + 1;
            }
            d = new short[FParityLength + 1 + 1];
            l = new short[FParityLength + 1 + 1];
            u_lu = new short[FParityLength + 1 + 1];
            s = new short[FParityLength + 1];
            root = new short[Convert.ToInt64(FParityLength / 2) - 1 + 1];
            loc = new short[Convert.ToInt64(FParityLength / 2) - 1 + 1];
            z = new short[Convert.ToInt64(FParityLength / 2) + 1];
            err = new short[FRSLength - 1 + 1];
            reg = new short[Convert.ToInt64(FParityLength / 2) + 1];
            for (nI = 0; nI <= (FParityLength - 1); nI++)
            {
                recd[nI] = xParity[nI];
            }
            //for (nI = 0; nI <= (FRSLength - 1); nI++) {
            for (nI = 0; nI <= (FDataLength - 1); nI++)
            {
                recd[FParityLength + nI] = xData[nI];
            }
            for (i = 0; i <= (FRSLength - 1); i++)
            {
                recd[i] = VectorToExp[recd[i]];
            }
            // put recd[i] into index form
            count = 0;
            syn_error = 0;
            result = 0;
            // first form the syndromes
            for (i = 1; i <= FParityLength; i++)
            {
                s[i] = 0;
                for (j = 0; j <= (FRSLength - 1); j++)
                {
                    if (recd[j] != -1)
                    {
                        // recd[j] in index form
                        s[i] = (short)(s[i] ^ ExpToVector[(recd[j] + i * j) % FMaxRSLength]);
                    }
                }
                // convert syndrome from polynomial form to index form
                if ((s[i] != 0))
                {
                    syn_error = 1;
                    // set flag if non-zero syndrome => error
                }
                s[i] = VectorToExp[s[i]];
            }
            if (syn_error != 0)
            {
                // if errors, try and correct
                // Compute the error location polynomial via the Berlekamp
                // iterative algorithm, following the terminology of Lin and
                // Costello :   d[u] is the 'mu'th discrepancy, where u='mu'+1
                // and 'mu' (the Greek letter!) is the step number ranging from
                // -1 to 2*tt (see L&C),  l[u] is the degree of the elp at that
                // step, and u_l[u] is the difference between the step number
                // and the degree of the elp.
                // Initialize table entries
                d[0] = 0;
                // index form
                d[1] = s[1];
                // index form
                elp[0][0] = 0;
                // index form
                elp[1][0] = 1;
                // polynomial form
                for (i = 1; i <= (FParityLength - 1); i++)
                {
                    elp[0][i] = -1;
                    // index form
                    elp[1][i] = 0;
                    // polynomial form
                }
                l[0] = 0;
                l[1] = 0;
                u_lu[0] = -1;
                u_lu[1] = 0;
                u = 0;
                while (((u < FParityLength) && (l[u + 1] <= Convert.ToInt64(FParityLength / 2))))
                {
                    u++;
                    if ((d[u] == -1))
                    {
                        l[u + 1] = l[u];
                        for (i = 0; i <= l[u]; i++)
                        {
                            elp[u + 1][i] = elp[u][i];
                            elp[u][i] = VectorToExp[elp[u][i]];
                        }
                    }
                    else
                    {
                        // search for words with greatest u_lu[q] for which d[q]!=0
                        q = (short)(u - 1);
                        while (((d[q] == -1) && (q > 0)))
                        {
                            q -= 1;
                        }
                        // have found first non-zero d[q]
                        if ((q > 0))
                        {
                            j = q;
                            while (j > 0)
                            {
                                j -= 1;
                                if (((d[j] != -1) && (u_lu[q] < u_lu[j])))
                                {
                                    q = (short)j;
                                }
                            }
                        }
                        // have now found q such that d[u]!=0 and u_lu[q] is maximum
                        // store degree of new elp polynomial
                        if ((l[u] > l[q] + u - q))
                        {
                            l[u + 1] = l[u];
                        }
                        else
                        {
                            l[u + 1] = (short)(l[q] + u - q);
                        }
                        // form new elp(x)
                        for (i = 0; i <= (FParityLength - 1); i++)
                        {
                            elp[u + 1][i] = 0;
                        }
                        for (i = 0; i <= l[q]; i++)
                        {
                            // if (elp[q][i] <> -1) then elp[u+1][i+u-q] := ExpToVector[(d[u] + FRSLength - d[q] + elp[q][i]) mod FMaxRSLength] ;
                            if ((elp[q][i] != -1))
                            {
                                elp[u + 1][i + u - q] = ExpToVector[(d[u] + FMaxRSLength - d[q] + elp[q][i]) % FMaxRSLength];
                            }
                        }
                        for (i = 0; i <= l[u]; i++)
                        {
                            elp[u + 1][i] = (short)(elp[u + 1][i] ^ elp[u][i]);
                            // convert old elp value to index
                            elp[u][i] = VectorToExp[elp[u][i]];
                        }
                    }
                    u_lu[u + 1] = (short)(u - l[u + 1]);
                    // form (u+1)th discrepancy
                    if (u < FParityLength)
                    {
                        // no discrepancy computed on last iteration
                        if ((s[u + 1] != -1))
                        {
                            d[u + 1] = ExpToVector[s[u + 1]];
                        }
                        else
                        {
                            d[u + 1] = 0;
                        }
                        for (i = 1; i <= l[u + 1]; i++)
                        {
                            if (((s[u + 1 - i] != -1) && (elp[u + 1][i] != 0)))
                            {
                                d[u + 1] = (short)(d[u + 1] ^ ExpToVector[(s[u + 1 - i] + VectorToExp[elp[u + 1][i]]) % FMaxRSLength]);
                            }
                        }
                        // put d[u+1] into index form
                        d[u + 1] = VectorToExp[d[u + 1]];
                    }
                }
                // end While
                u++;
                if (l[u] <= Convert.ToInt64(FParityLength / 2))
                {
                    // can correct error
                    // put elp into index form
                    for (i = 0; i <= l[u]; i++)
                    {
                        elp[u][i] = VectorToExp[elp[u][i]];
                    }
                    // find roots of the error location polynomial
                    for (i = 1; i <= l[u]; i++)
                    {
                        reg[i] = elp[u][i];
                    }
                    // for i := 1 to FRSLength do begin
                    for (i = 1; i <= FMaxRSLength; i++)
                    {
                        q = 1;
                        for (j = 1; j <= l[u]; j++)
                        {
                            if (reg[j] != -1)
                            {
                                reg[j] = (short)((reg[j] + j) % FMaxRSLength);
                                q = (short)(q ^ ExpToVector[reg[j]]);
                            }
                        }
                        if (q == 0)
                        {
                            // store root and error location number indices
                            root[count] = (short)i;
                            // loc[count] := FRSLength - i ;
                            loc[count] = (short)(FMaxRSLength - i);
                            count++;
                        }
                    }
                    if (count == l[u])
                    {
                        // no. roots = degree of elp hence <= tt errors
                        result = count;
                        // form polynomial z(x)
                        for (i = 1; i <= l[u]; i++)
                        {
                            // Z[0] = 1 always - do not need
                            if (((s[i] != -1) && (elp[u][i] != -1)))
                            {
                                z[i] = (short)(ExpToVector[s[i]] ^ ExpToVector[elp[u][i]]);
                            }
                            else if (((s[i] != -1) && (elp[u][i] == -1)))
                            {
                                z[i] = ExpToVector[s[i]];
                            }
                            else if (((s[i] == -1) && (elp[u][i] != -1)))
                            {
                                z[i] = ExpToVector[elp[u][i]];
                            }
                            else
                            {
                                z[i] = 0;
                            }
                            for (j = 1; j <= (i - 1); j++)
                            {
                                if (((s[j] != -1) && (elp[u][i - j] != -1)))
                                {
                                    z[i] = (short)(z[i] ^ ExpToVector[(elp[u][i - j] + s[j]) % FMaxRSLength]);
                                }
                            }
                            // put into index form
                            z[i] = VectorToExp[z[i]];
                        }
                        // evaluate errors at locations given by
                        // error location numbers loc[i]
                        for (i = 0; i <= (FRSLength - 1); i++)
                        {
                            // for i := 0 to (FMaxRSLength - 1) do begin
                            err[i] = 0;
                            // convert recd[] to polynomial form
                            if (recd[i] != -1)
                            {
                                recd[i] = ExpToVector[recd[i]];
                            }
                            else
                            {
                                recd[i] = 0;
                            }
                        }
                        for (i = 0; i <= (l[u] - 1); i++)
                        {
                            // compute numerator of error term first
                            err[loc[i]] = 1;
                            // accounts for z[0]
                            for (j = 1; j <= l[u]; j++)
                            {
                                if (z[j] != -1)
                                {
                                    err[loc[i]] = (short)(err[loc[i]] ^ ExpToVector[(z[j] + j * root[i]) % FMaxRSLength]);
                                }
                            }
                            if (err[loc[i]] != 0)
                            {
                                err[loc[i]] = VectorToExp[err[loc[i]]];
                                q = 0;
                                // form denominator of error term
                                for (j = 0; j <= (l[u] - 1); j++)
                                {
                                    if (j != i)
                                    {
                                        q = (short)(q + VectorToExp[1 ^ ExpToVector[(loc[j] + root[i]) % FMaxRSLength]]);
                                    }
                                }
                                q = (short)(q % FMaxRSLength);
                                // err[loc[i]] := ExpToVector[(err[loc[i]] - q + FRSLength) mod FMaxRSLength] ;
                                err[loc[i]] = ExpToVector[(err[loc[i]] - q + FMaxRSLength) % FMaxRSLength];
                                // recd[i] must be in polynomial form
                                recd[loc[i]] = (short)(recd[loc[i]] ^ err[loc[i]]);
                            }
                        }
                    }
                    else
                    {
                        // no. roots != degree of elp => >tt errors and cannot solve
                        result = -1;
                        // Signal an error.
                        for (i = 0; i <= (FRSLength - 1); i++)
                        {
                            // could return error flag if desired
                            if (recd[i] != -1)
                            {
                                // convert recd[] to polynomial form
                                recd[i] = ExpToVector[recd[i]];
                            }
                            else
                            {
                                recd[i] = 0;
                            }
                            // just output received codeword as is
                        }
                    }
                }
                else
                {
                    // if l[u] <= tt then
                    // elp has degree has degree >tt hence cannot solve
                    for (i = 0; i <= (FRSLength - 1); i++)
                    {
                        // could return error flag if desired
                        if (recd[i] != -1)
                        {
                            // convert recd[] to polynomial form
                            recd[i] = ExpToVector[recd[i]];
                        }
                        else
                        {
                            recd[i] = 0;
                        }
                        // just output received codeword as is
                    }
                }
            }
            else
            {
                // If syn_error <> 0 then
                // no non-zero syndromes => no errors: output received codeword
                for (i = 0; i <= (FRSLength - 1); i++)
                {
                    if (recd[i] != -1)
                    {
                        // convert recd[] to polynomial form
                        recd[i] = ExpToVector[recd[i]];
                    }
                    else
                    {
                        recd[i] = 0;
                    }
                    result = 0;
                    // No errors ocurred.
                }
            }

            // for nI := FParityLength to (FRSLength - 1) do xDecoded[nI-FParityLength] := Recd[nI] ;
            for (nI = FParityLength; nI <= (FRSLength - 1); nI++)
            {
                xDecoded[nI - FParityLength] = (byte)recd[nI];
            }
            return result;
        }

        public void SetGFDimension(int value)
        {
            FGFDimension = value;
            //갈루아 체의 차원이 바뀌면 모든 다시 만들 필요가있다
            ReInitParameters(FGFDimension, FParityLength, FRSLength);
        }

        public void SetRSLength(int RSLength, int ParityLength)
        {
            FRSLength = RSLength;
            FParityLength = ParityLength;
            FDataLength = FRSLength - FParityLength;  //data symbols, kk = nn-2*MaxErrors

            gg = new short[FParityLength + 1];
            bb = new short[FParityLength];
            data = new short[FDataLength + 1];
            CodeWord = new byte[FRSLength - 1 + 1];
            recd = new short[FRSLength - 1 + 1];

            gen_poly();
        }

        public void SetRSMaxLength()
        {
            int ml;
            ml = (1 << FGFDimension) - 1;
            // nn=2**mm -1   length of codeword
            //RSLength = ml;
            SetRSLength(ml, FParityLength);
        }
    } // end class iReedSolomon
} // end namespace
