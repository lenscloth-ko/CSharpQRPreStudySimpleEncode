using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpReedSolomon
{
    /// <summary>
    /// Reed Solomon 부호화 구현 클래스 (다른 버전)
    /// </summary>
    public class iReedSolomonCodec
    {
        private int NPAR = 0;
        private int MAXDEG = 0;
        private int FDimension = 0;
        private iGalois GT = null;
        private int FNErasures = 0;
        private int[] ErasureLocs;
        private int[] ErrorLocs;
        private int NErrors = 0;
        private int[] Lambda;
        // [MAXDEG];
        private int[] Omega;
        public int[] pBytes;
        public int[] synBytes;
        public int[] genPoly;

        public int Dimension
        {
            get
            {
                return FDimension;
            }
            set
            {
                // [MAXDEG];
                if (FDimension != value)
                {
                    FDimension = value;
                }
            }
        }

        //public TiPentecReedSolomonCodec(Component Aowner) : base(Aowner)
        public iReedSolomonCodec()
        {
            FDimension = 8;
            GT = new iGalois();
        }

        /*
        protected override void Dispose()
        {

        }
        */

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //  초기화
        public void initialize_ecc(int ParityLength)
        {
            NPAR = ParityLength;
            MAXDEG = NPAR * 2;
            // * Initialize the galois field arithmetic tables */
            GT.InitTables(FDimension);
            // init_galois_tables();
            // * Compute the encoder generator polynomial */
            genPoly = new int[MAXDEG * 2];
            compute_genpoly(NPAR, out genPoly);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //  Decode
        //디코딩 함수
        //디코딩 함수를 호출 한 후 check_syndrome에서 신드롬을 요구
        public void decode_data(ref char[] data, int nbytes)
        {
            int i;
            int j;
            int sum;
            synBytes = new int[MAXDEG];
            for (j = 0; j < NPAR; j++)
            {
                // for j:=0 to 8-1 do begin
                sum = 0;
                for (i = 0; i < nbytes; i++)
                {
                    sum = ((byte)data[i]) ^ GT.gmult(GT.gexp[j + 1], sum);
                }
                synBytes[j] = sum;
            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //  Check Syndrome
        //    증후군 산출
        //    Decode를 사전에 호출 할 필요가있다
        public int check_syndrome()
        {
            int result;
            int i;
            int nz;
            nz = 0;
            for (i = 0; i < NPAR; i++)
            {
                if (synBytes[i] != 0)
                {
                    nz = 1;
                }
            }
            result = nz;
            return result;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //  Encode
        //    인코딩 함수 코드 워드 버전
        public void encode_data(char[] msg, int nbytes, out char[] dst)
        {
            int i;
            int dbyte;
            int j;
            int[] LFSR;
            LFSR = new int[NPAR + 1];
            for (i = 0; i <= NPAR; i++)
            {
                LFSR[i] = 0;
            }
            for (i = 0; i < nbytes; i++)
            {
                dbyte = ((byte)msg[i]) ^ LFSR[NPAR - 1];
                for (j = NPAR - 1; j >= 1; j--)
                {
                    LFSR[j] = LFSR[j - 1] ^ GT.gmult(genPoly[j], dbyte);
                }
                LFSR[0] = GT.gmult(genPoly[0], dbyte);
            }
            pBytes = new int[NPAR];
            for (i = 0; i < NPAR; i++)
            {
                pBytes[i] = LFSR[i];
            }
            build_codeword(msg, nbytes, out dst);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //  Encode Parity
        //    인코딩 함수 패리티 분리 판
        public void encode_data_parity(char[] msg, int nbytes, out char[] Parity)
        {
            int i;
            int dbyte;
            int j;
            int[] LFSR;
            LFSR = new int[NPAR + 1];
            for (i = 0; i <= NPAR; i++)
            {
                LFSR[i] = 0;
            }
            for (i = 0; i < nbytes; i++)
            {
                dbyte = ((byte)msg[i]) ^ LFSR[NPAR - 1];
                for (j = NPAR - 1; j >= 1; j--)
                {
                    LFSR[j] = LFSR[j - 1] ^ GT.gmult(genPoly[j], dbyte);
                }
                LFSR[0] = GT.gmult(genPoly[0], dbyte);
            }

            Parity = new char[NPAR];
            pBytes = new int[NPAR];
            for (i = 0; i < NPAR; i++)
            {
                Parity[i] = ((char)LFSR[i]);
            }
            // for i := 0 to NPAR-1 do Pareity := pBytes[i];

        }

        public short crc_ccitt(string msg, int leng)
        {
            short result = 0;
            return result;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //  correct errors erasures
        //    오류 정정 처리
        public int correct_errors_erasures(ref char[] codeword, int csize, int nerasures, int[] erasures)
        {
            int result;
            int r;
            int i;
            int j;
            byte err;
            int num;
            int denom;
            FNErasures = nerasures;
            for (i = 0; i < nerasures; i++)
            {
                ErasureLocs[i] = erasures[i];
            }
            Modified_Berlekamp_Massey();
            Find_Roots();
            if ((NErrors <= NPAR) && (NErrors > 0))
            {
                // /* first check for illegal error locs */
                for (r = 0; r < NErrors; r++)
                {
                    if (ErrorLocs[r] >= csize)
                    {
                        result = 0;
                        return result;
                    }
                }
                for (r = 0; r < NErrors; r++)
                {
                    i = ErrorLocs[r];
                    // /* evaluate Omega at alpha^(-i) */
                    num = 0;
                    for (j = 0; j < MAXDEG; j++)
                    {
                        num = num ^ GT.gmult(Omega[j], GT.gexp[((255 - i) * j) % 255]);
                    }
                    // /* evaluate Lambda' (derivative) at alpha^(-i) ; all odd powers disappear */
                    denom = 0;
                    j = 1;
                    while ((j < MAXDEG))
                    {
                        denom = denom ^ GT.gmult(Lambda[j], GT.gexp[((255 - i) * (j - 1)) % 255]);
                        j += 2;
                    }
                    err = (byte)GT.gmult(num, GT.ginv(denom));
                    //codeword[csize - i - 1] = ((char)((byte)codeword[csize - i - 1]) ^ err);
                    byte b = (byte)codeword[csize - i - 1];
                    b = (byte)(b ^ err);
                    codeword[csize - i - 1] = (char)b;
                }
                result = 1;
                return result;
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //  Compute GenPoly
        //    오류 정정 함수 작성
        //    오류 정정 함수는 인코딩시에만 사용되는
        protected void compute_genpoly(int nbytes, out int[] genpoly)
        {
            genpoly = new int[0];

            int i;
            int[] tp = new int[255 + 1];
            int[] tp1 = new int[255 + 1];
            // /* multiply (x + a^n) for n = 1 to nbytes */
            zero_poly(out tp1);
            tp1[0] = 1;
            for (i = 1; i <= nbytes; i++)
            {
                zero_poly(out tp);
                tp[0] = GT.gexp[i];
                // * set up x+a^n */
                tp[1] = 1;
                mult_polys(out genpoly, tp, tp1);
                copy_poly(out tp1, genpoly);
            }
            // QR 코드에 대한 오류 정정 다항식
            // for i := 0 to nbytes do begin
            // genpoly[i]:=GT.gexp[GT.glog[genpoly[i]] - (nbytes-i)];
            // end;

        }

        protected void build_codeword(char[] msg, int nbytes, out char[] dst)
        {
            int i;
            dst = new char[nbytes];
            for (i = 0; i < nbytes; i++)
            {
                dst[i] = msg[i];
            }
            for (i = 0; i < NPAR; i++)
            {
                dst[i + nbytes] = ((char)pBytes[NPAR - 1 - i]);
            }
        }

        protected void Modified_Berlekamp_Massey()
        {
            int n;
            int l;
            int l2;
            int k;
            int d;
            int i;
            int[] psi;
            int[] psi2;
            int[] DA;
            int[] gamma;
            psi = new int[MAXDEG];
            psi2 = new int[MAXDEG];
            DA = new int[MAXDEG];
            gamma = new int[MAXDEG];
            // /* initialize Gamma, the erasure locator polynomial */
            init_gamma(out gamma);
            // /* initialize to z */
            copy_poly(out DA, gamma);
            mul_z_poly(ref DA);
            copy_poly(out psi, gamma);
            k = -1;
            l = FNErasures;
            for (n = FNErasures; n < NPAR; n++)
            {
                // for n:=FNErasures to 8-1 do begin
                d = compute_discrepancy(psi, synBytes, l, n);
                if (d != 0)
                {
                    // /* psi2 = psi - d*D */
                    for (i = 0; i < MAXDEG; i++)
                    {
                        psi2[i] = psi[i] ^ GT.gmult(d, DA[i]);
                    }
                    if (l < (n - k))
                    {
                        l2 = n - k;
                        k = n - l;
                        // /* D = scale_poly(ginv(d), psi); */
                        for (i = 0; i < MAXDEG; i++)
                        {
                            DA[i] = GT.gmult(psi[i], GT.ginv(d));
                        }
                        l = l2;
                    }
                    // /* psi = psi2 */
                    for (i = 0; i < MAXDEG; i++)
                    {
                        psi[i] = psi2[i];
                    }
                }
                mul_z_poly(ref DA);
            }
            // λ의 초기화
            Lambda = new int[MAXDEG];
            for (i = 0; i < MAXDEG; i++)
            {
                Lambda[i] = psi[i];
            }
            compute_modified_omega();
        }

        // * gamma = product (1-z*a^Ij) for erasure locs Ij */
        protected void init_gamma(out int[] gamma)
        {
            int e;
            int[] tmp;
            tmp = new int[MAXDEG];
            zero_poly(out gamma);
            zero_poly(out tmp);
            gamma[0] = 1;
            for (e = 0; e < FNErasures; e++)
            {
                copy_poly(out tmp, gamma);
                scale_poly(GT.gexp[ErasureLocs[e]], out tmp);
                mul_z_poly(ref tmp);
                add_polys(out gamma, tmp);
            }
        }

        protected int compute_discrepancy(int[] lambda, int[] S, int L, int n)
        {
            int result;
            int i;
            int sum;
            sum = 0;
            for (i = 0; i <= L; i++)
            {
                sum = sum ^ GT.gmult(lambda[i], S[n - i]);
            }
            result = sum;
            return result;
        }

        protected void compute_modified_omega()
        {
            int i;
            int[] product;
            product = new int[MAXDEG * 2];
            mult_polys(out product, Lambda, synBytes);
            // ω 초기화 (0으로 클리어 후 product 초기 값을 대입)
            Omega = new int[MAXDEG];
            zero_poly(out Omega);
            for (i = 0; i < NPAR; i++)
            {
                Omega[i] = product[i];
            }
            //this.Finalize(product);
        }

        protected void Find_Roots()
        {
            int sum;
            int r;
            int k;
            NErrors = 0;
            for (r = 1; r < 256; r++)
            {
                sum = 0;
                // /* evaluate lambda at r */
                for (k = 0; k < NPAR + 1; k++)
                {
                    sum = sum ^ GT.gmult(GT.gexp[(k * r) % 255], Lambda[k]);
                }
                if ((sum == 0))
                {
                    ErrorLocs[NErrors] = 255 - r;
                    NErrors++;
                }
            }
        }

        public void zero_poly(out int[] poly)
        {
            int i;
            poly = new int[MAXDEG];
            for (i = 0; i < MAXDEG; i++)
            {
                poly[i] = 0;
            }
        }

        public void copy_poly(out int[] dst, int[] src)
        {
            int i;
            dst = new int[MAXDEG];
            for (i = 0; i < MAXDEG; i++)
            {
                dst[i] = src[i];
            }
        }

        public void add_polys(out int[] dst, int[] src)
        {
            int i;
            dst = new int[MAXDEG];
            for (i = 0; i < MAXDEG; i++)
            {
                dst[i] = dst[i] ^ src[i];
            }
        }

        // /* multiply by z, i.e., shift right by 1 */
        protected void mul_z_poly(ref int[] src)
        {
            int i;
            src = new int[MAXDEG - 1];
            for (i = MAXDEG - 1; i >= 1; i--)
            {
                src[i] = src[i - 1];
            }
            src[0] = 0;
        }

        public void scale_poly(int k, out int[] poly)
        {
            int i;
            poly = new int[MAXDEG];
            for (i = 0; i < MAXDEG; i++)
            {
                poly[i] = GT.gmult(k, poly[i]);
            }
        }

        public void mult_polys(out int[] dst, int[] p1, int[] p2)
        {
            int i;
            int j;
            int[] tmp1;
            tmp1 = new int[MAXDEG * 2];

            dst = new int[MAXDEG * 2];
            for (i = 0; i < (MAXDEG * 2); i++)
            {
                dst[i] = 0;
            }

            for (i = 0; i < MAXDEG; i++)
            {
                for (j = MAXDEG; j < (MAXDEG * 2); j++)
                {
                    tmp1[j] = 0;
                }
                // /* scale tmp1 by p1[i] */
                for (j = 0; j < MAXDEG; j++)
                {
                    tmp1[j] = GT.gmult(p2[j], p1[i]);
                }
                // /* and mult (shift) tmp1 right by i */
                j = (MAXDEG * 2) - 1;
                while ((j >= i))
                {
                    tmp1[j] = tmp1[j - i];
                    j -= 1;
                }
                for (j = 0; j < i; j++)
                {
                    tmp1[j] = 0;
                }
                // /* add into partial product */
                for (j = 0; j < (MAXDEG * 2); j++)
                {
                    dst[j] = dst[j] ^ tmp1[j];
                }
            }
        }
    } // end class iReedSolomonCodec
}
