using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpReedSolomon
{
    /// <summary>
    /// 갈루아 체 원시 다항식 구현 클래스
    /// https://www.ipentec.com/document/csharp-reed-solomon-implementation
    /// </summary>
    public class iGalois
    {
        private byte[] PP;
        // specify irreducible polynomial coeffts
        // 갈루아 체 원시 다항식
        private byte[][] pPP = new byte[16 + 1][];
        private int FTableLength = 0;
        public int[] gexp;
        public int[] glog;


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
        public static byte[] PP16 = { 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1 };

        public iGalois()
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
        }

        public void InitTables(int Dimension)
        {
            // * initialize the table of powers of alpha */
            // init_exp_table();
            init_exp_table(Dimension);
        }

        private void init_exp_table(int Dimension)
        {
            int i;
            short mask;
            FTableLength = (1 << Dimension);
            gexp = new int[FTableLength];
            glog = new int[FTableLength];
            SetPrimitive(ref PP, Dimension);
            mask = 1;
            gexp[Dimension] = 0;
            for (i = 0; i <= (Dimension - 1); i++)
            {
                gexp[i] = mask;
                glog[gexp[i]] = i;
                if (PP[i] != 0)
                {
                    gexp[Dimension] = gexp[Dimension] ^ mask;
                }
                mask = (short)(mask << 1);
            }
            glog[gexp[Dimension]] = Dimension;
            mask = (short)(mask >> 1);
            for (i = (Dimension + 1); i <= (FTableLength - 1); i++)
            {
                if (gexp[i - 1] >= mask)
                {
                    gexp[i] = gexp[Dimension] ^ ((gexp[i - 1] ^ mask) << 1);
                }
                else
                {
                    gexp[i] = gexp[i - 1] << 1;
                }
                glog[gexp[i]] = i;
            }
        }

        private void SetPrimitive(ref byte[] PP, int nIdx)
        {
            Array.Copy(PP, nIdx, PP, 0, (nIdx + 1));
            //@ Unsupported function or procedure: 'Move'
            //Move(PP[nIdx], PP, (nIdx + 1));
        }

        // /* multiplication using logarithms */
        public int gmult(int a, int b)
        {
            int result;
            int i;
            int j;
            if ((a == 0) || (b == 0))
            {
                result = 0;
            }
            else
            {
                i = glog[a];
                j = glog[b];
                // result:=gexp[i+j];                  //테이블의 길이가 2 배의 때
                result = gexp[(i + j) % (FTableLength - 1)];
                // 테이블의 길이가 적합한 경우
            }
            return result;
        }

        public int ginv(int elt)
        {
            int result;
            result = gexp[255 - glog[elt]];
            return result;
        }
    } // end class iGalois
} // end namespace
