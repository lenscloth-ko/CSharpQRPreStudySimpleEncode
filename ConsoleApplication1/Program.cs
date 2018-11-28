using CSharpQRPreStudySimpleEncode.Steps;
using CSharpQRStudySimpleEncode.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();

            //2.2 데이터 코드로 인코딩
            string iVersion = "1"; //버전
            string iErrorRestoreLevel = "H"; // 오류 정정 단계
            string iStr = "ABCDE123"; //입력문자를 이걸로 가정 버전 1, 오류 정정 단계 H(이하 1-H라 한다.) 

            //2.2.1 모드 지시자
            //숫자 모드 : 0001
            //알파벳(대문자만) + α 모드: 0010
            //8bit 모드: 0100
            //KANJI 모드 : 1000

            string ModeIndicator = "0010";  //ABCDE123 는 알파벳+a => 0010

            Console.WriteLine("ModeIndicator : " + ModeIndicator);
            sb.Append(ModeIndicator);

            //2.2.2 데이터 갯수 지시자
            //숫자: 10bit
            //알파벳(대문자만) + α : 9bit
            //8bit: 8bit
            //KANJI : 8bit

            int x = 8;
            string s = Convert.ToString(x, 2);

            //ABCDE123 은 8문자임으로 8을 9bit(알파벳 + α) 바이너리 데이터로 인코딩 해야함

            int[] bits = s.PadLeft(9, '0')
                .Select(c => int.Parse(c.ToString()))
                .ToArray();

            foreach (int item in bits)
            {
                //EncodingData.Add(item.ToString());
                Console.Write(item);
                sb.Append(item.ToString());
            }
            Console.WriteLine("");

            //2.2.3 데이터 인코딩
            //숫자 모드에서는, 데이터는 각각 3자리의 숫자들로 나뉜다.
            //예로 "123456"은 "123"과 "456"으로 나뉜다. 순서는 바뀌지 않는다.
            //또한 각각의 나뉜 데이터는 10bit 바이너리 데이터로 인코딩된다.

            //만약 나뉜 데이터에 숫자가 1개 또는 2개라면, 각각 4bit와 7bit의 바이너리 데이터로 인코딩해야 한다.
            //예로 "9876"은 "987"과 "6"으로 나뉘는데, "987"은 10bit, "6"은 4bit 바이너리 데이터로 인코딩한다.
            //이것의 결과는 "1111011011 0110"이 된다.

            //알파벳(대문자만) + α 모드에서는, 표2에 의해 모든 문자가 특정한 숫자로 바뀐다.
            //그 다음 모든 숫자들을 2개씩 나눈다. 2개 중 첫 번째 값에 45를 
            //곱하고 두 번째 값을 더한 결과를 11bit 바이너리 데이터로 인코딩하면 된다.
            //만약 이 결과가 10보다 작을 때에는 6bit 바이너리 데이터로 인코딩해야 한다.
            DataEncodingModeAlphabatPlusAlpha demap = new DataEncodingModeAlphabatPlusAlpha(iStr);
            Console.WriteLine("DataEncodingModeAlphabatPlusAlpha : " + demap.GetEncodingData);
            sb.Append(demap.GetEncodingData);

            //2.2.4 종료 지시자
            //2.2.3에서 나온 결과에 0000을 추가한다. 
            //만약 2.2.3에서 이미 데이터 코드의 길이가 최대에 도달했다면 이 작업은 할 필요가 없다.
            DataMaximumLengthPerModel dmlp = new DataMaximumLengthPerModel(iStr, iVersion, iErrorRestoreLevel, ModeIndicator);
            //Console.WriteLine(dmlp.GetMaximumLengthPerModel);
            string EndIndicator = dmlp.GetMaximumLengthPerModel;

            Console.WriteLine("DataMaximumLengthPerModel : " + EndIndicator);
            sb.Append(EndIndicator);


            //2.2.5 데이터 코드로 인코딩
            //2.2.4에서 나온 결과를 8bit 단위로 나눈다.
            //test print

            for (int i = 0; i < sb.ToString().Length; i++)
            {
                Console.Write(sb[i].ToString());
                if (((i + 1) % 8) == 0)
                {
                    Console.Write(" ");
                }
            }

            //만약 마지막 8bit가 모두 차지 않았다면 남은 bit를 모두 0으로 채운다.
            //Console.WriteLine("\n남은 bit start");
            //Console.WriteLine(8 - (sb.ToString().Length % 8));
            int curLength = (8 - (sb.ToString().Length % 8));
            for (int i = 0; i < curLength; i++)
            {
                //Console.Write(" 0" + i + "번째");
                sb.Append("0");
            }
            //sb.Append("000");
            //Console.WriteLine("\n남은 bit end");

            //Console.WriteLine("\nX : " + (sb.ToString().Length % 8));

            //EndIndicator 까지 더해서 남은 비트를 체운상태의 문자열이 지금까지 sb에 담겨져있다
            //여기서 8bit로 변환하고 블럭수를 해당 버전과 정정단계 코드값으로 최대 블럭수를 받아와서 비교한다

            //버전과 정정코드별 최대 데이터 코드 갯수(8bit block의 수)
            int max8bitBlockSize = int.Parse(dmlp.GetMax8bitBlock);

            //int a = sb.ToString().Length;
            //int b = 8;
            //double d = a / b;

            Console.WriteLine("전체 block수 : " + max8bitBlockSize);
            //Console.WriteLine("몫 : " + System.Math.Truncate(d));
            //Console.WriteLine("나머지 : " + (a & b));

            Console.WriteLine("현재 block수 : " + sb.ToString().Length / 8);

            //만약 데이터 코드가 표1의 최대 개수만큼 차지 못했다면 
            //"11101100"과 "00010001"을 번갈아 가면서 최대 개수까지 추가해준다.

            //max8bitBlockSize = 20;
            for (int i = 0; i < max8bitBlockSize - (sb.ToString().Length / 8); i++)
            {
                if ((i % 2) == 0)
                {
                    Console.WriteLine("짝수 더한다" + i);
                    sb.Append("11101100");
                }
                else
                {
                    Console.WriteLine("홀수 더한다" + i);
                    sb.Append("00010001");
                }
            } // end for

            Console.WriteLine("\n-------------------------");
            Console.WriteLine(sb.ToString());

            //2진수를 10진수로 변환해서 검수
            Console.WriteLine(Convert.ToByte("00100000", 2));

            string _loopString = string.Empty;

            Console.WriteLine("\n----------------------result");
            for (int i = 0; i < sb.ToString().Length; i++)
            {
                _loopString += sb[i].ToString();

                if (((i + 1) % 8) == 0)
                {
                    Console.WriteLine(
                        string.Format("{0} -> 10 decimal : {1}"
                        , _loopString
                        , Convert.ToByte(_loopString, 2))
                        );
                    _loopString = string.Empty;
                }
            } // end for


            //하? 이제 Reed Solomon 관련인데...

            //2.3 오류 정정 코드 계산
            //QR코드의 오류 정정에는 Reed-solomon 알고리즘이 사용된다.
            //먼저, 전 단계에서 나온 결과를 표5의 규칙에 의해 RS 블록으로 분할한다.
            //예제 데이터는 1 - H를 사용하는데 이는 RS 블록 1이므로 분할할 필요는 없다.

            //00100000-> 10 decimal : 32
            //01000001-> 10 decimal : 65
            //11001101-> 10 decimal : 205
            //01000101-> 10 decimal : 69
            //00101001-> 10 decimal : 41
            //11011100-> 10 decimal : 220
            //00101110-> 10 decimal : 46
            //10000000-> 10 decimal : 128
            //11101100-> 10 decimal : 236

            int[] cData = new int[sb.ToString().Length];

            for (int i = 0; i < sb.ToString().Length; i++)
            {
                cData[i] = int.Parse(sb.ToString()[i].ToString());
            }

            foreach (int item in cData)
            {
                Console.Write(item);
            }

            Console.WriteLine("\n-----------------------------------Reed Solomon Start----");

            // 오류 정정 코드의 크기!!
            int errorRepairCodeCount;
            //RS블럭 구조 필드 iReedSolomon(GFDimension, RSLength, ParityLength)
            int GFDimension, RSLength, ParityLength;

            ReedSolomonBlock RSBlock = new ReedSolomonBlock(iVersion, iErrorRestoreLevel);
            errorRepairCodeCount = RSBlock.GetErrorRepairCodeCount;
            RSLength = RSBlock.GetRSLength;
            GFDimension = RSBlock.GetGFDimension;
            ParityLength = RSBlock.GetParityLength;

            Console.WriteLine("errorRepairCodeCount = " + errorRepairCodeCount);
            Console.WriteLine("GFDimension = " + GFDimension);
            Console.WriteLine("RSLength = " + RSLength);
            Console.WriteLine("ParityLength = " + ParityLength);

            #region 2차시도 ㄱㄱ
            //GenericGF gf = new GenericGF(8, 27, 0);
            //Console.WriteLine("gf.Size = " + gf.Size);
            //Console.WriteLine("gf.GeneratorBase = " + gf.GeneratorBase);

            //ReedSolomonEncoder rsEnc = new ReedSolomonEncoder(gf);
            //Console.WriteLine(rsEnc.ToString());
            #endregion

            #region 1차시도 ㅠㅠ 검증값 불일치
            ////오류 정정 코드의 크기!!
            //int errorRepairCodeCount;
            ////RS블럭 구조 필드 iReedSolomon(GFDimension, RSLength, ParityLength)
            //int GFDimension, RSLength, ParityLength;

            //ReedSolomonBlock RSBlock = new ReedSolomonBlock(iVersion, iErrorRestoreLevel);
            //errorRepairCodeCount = RSBlock.GetErrorRepairCodeCount;
            //RSLength = RSBlock.GetRSLength;
            //GFDimension = RSBlock.GetGFDimension;
            //ParityLength = RSBlock.GetParityLength;

            //Console.WriteLine("errorRepairCodeCount = " + errorRepairCodeCount);
            //Console.WriteLine("GFDimension = " + GFDimension);
            //Console.WriteLine("RSLength = " + RSLength);
            //Console.WriteLine("ParityLength = " + ParityLength);

            //byte[] data = new byte[2550];

            //for (int i = 0; i < sb.ToString().Length; i++)
            //{
            //    data[i] = (byte)sb.ToString()[i];
            //    //Console.WriteLine(iStr[i]);
            //}

            ////reed-solomon encdata
            //byte[] encdata = new byte[2550];

            ////인코딩
            ////RS 블럭의 구조 (a,b,c)의 a, b, c는 각각 전체 코드의 수, 데이터 코드의 수, 오류 정정 코드의 수를 의미한다.
            ////iReedSolomon iprs = new iReedSolomon(8, 26, 7);
            //iReedSolomon iprs = new iReedSolomon(RSLength, GFDimension, ParityLength);
            //iprs.EncodeRS(ref data, ref encdata);


            ////iprs.DecodeRS(ref data, ref encdata);

            //foreach (byte item in encdata)
            //{
            //    Console.Write(item);
            //}
            #endregion



        } // end Main 
    } // end class Program 
} //  end namespace
