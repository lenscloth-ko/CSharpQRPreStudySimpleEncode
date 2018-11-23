using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CSharpQRPreStudySimpleEncode.Steps
{
    /// <summary>
    /// https://kennysoft.kr/qr_ko/qr2_kr.htm
    /// 2.2.3 데이터 인코딩
    /// 알파벳+α 모드에서는, 표2에 의해 모든 문자가 특정한 숫자로 바뀐다.
    /// 그 다음 모든 숫자들을 2개씩 나눈다.
    /// 2개 중 첫 번째 값에 45를 곱하고 두 번째 값을 더한 결과를 11bit 바이너리 데이터로 인코딩하면 된다. 
    /// 만약 나뉜 숫자가 1개라면 6bit 바이너리 데이터로 인코딩해야 한다.
    /// </summary>
    public class DataEncodingModeAlphabatPlusAlpha
    {
        /// <summary>
        /// 표2 의 데이터 테이블
        /// </summary>
        private DataTable QRAlphabatPlusAlphaTable = new DataTable("QRAlphabatPlusAlphaTable");

        /// <summary>
        /// 입력 데이터
        /// </summary>
        private string _inputData;
        /// <summary>
        /// 데이터 길이
        /// </summary>
        private int _dataLength;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="inputData"></param>
        public DataEncodingModeAlphabatPlusAlpha(string inputData)
        {
            init_QRAlphabatPlusAlphaTable();

            //문자열이 홀수라면 마지막에 @( 마지막1개는 6bit 바이터리로 처리하기위함)
            this._inputData = (inputData.Length % 2) != 0 ? inputData : inputData + "@";
        }

        /// <summary>
        /// I데이터 변환
        /// </summary>
        public string GetEncodingData
        {
            get { return EncodingData(); }
        } // end GetEncodingData

        /// <summary>
        /// 데이터 변환
        /// </summary>
        /// <returns></returns>
        private string EncodingData()
        {
            List<int> _adds = new List<int>();
            StringBuilder sb = new StringBuilder();
            List<int> _decimals = new List<int>();

            foreach (char item in _inputData)
            {
                //만약 나뉜 숫자가 1개라면 6bit 바이너리 데이터로 인코딩해야 한다.
                //rows = this.QRAlphabatPlusAlphaTable.Select(
                //    string.Format("Key = '{0}'", item.ToString())
                //    )[0]["Value"].ToString();
                _decimals.Add(
                    int.Parse(
                        this.QRAlphabatPlusAlphaTable.Select(
                            string.Format("Key = '{0}'", item.ToString()
                        ))[0]["Value"].ToString()
                        )
                    );
            }

            for (int i = 0; i < _decimals.Count; i++)
            {
                if (i % 2 == 0)
                {
                    Console.WriteLine("45 * " + _decimals[i].ToString());
                }
                else
                {
                    if (_decimals[i].Equals(99))
                    {
                        Console.WriteLine("6bit처리");
                        Console.WriteLine(GetBitsForLeftPad((45 * _decimals[i - 1]), 6));
                        sb.Append(GetBitsForLeftPad((45 * _decimals[i - 1]), 6));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("(45 * {0}) + {1} = {2}"
                            , _decimals[i - 1]
                            , _decimals[i]
                            , ((45 * _decimals[i - 1]) + _decimals[i])
                            ));
                        //Console.WriteLine(_decimals[i].ToString() + "을 더한다");
                        Console.WriteLine(GetBitsForLeftPad(((45 * _decimals[i - 1]) + _decimals[i]), 11));
                        sb.Append(GetBitsForLeftPad(((45 * _decimals[i - 1]) + _decimals[i]), 11));
                    }

                }
            }

            return sb.ToString();
        } // end EncodingData

        /// <summary>
        /// 선택 수를 padLength의 이진 bit로 변환
        /// </summary>
        /// <param name="inputNum">입력숫자</param>
        /// <param name="padLength">pad 길이</param>
        /// <returns></returns>
        private string GetBitsForLeftPad(int inputNum, int padLength)
        {
            StringBuilder sb = new StringBuilder();

            int[] bits = Convert.ToString(inputNum, 2).PadLeft(padLength, '0')
                .Select(c => int.Parse(c.ToString()))
                .ToArray();

            foreach (int bit in bits)
            {
                sb.Append(bit.ToString());
            }

            return sb.ToString();
        } // end GetBitsForLeftPad

        #region 표2. 알파벳+α 모드에서의 변환 테이블
        /// <summary>
        /// 표2. 알파벳+α 모드에서의 변환 테이블
        /// </summary>
        private void init_QRAlphabatPlusAlphaTable()
        {
            //대소문자 사용 해야하나???
            this.QRAlphabatPlusAlphaTable.CaseSensitive = false;

            //항목
            //QR 알파벳+α 모드에서의 변환 테이블
            this.QRAlphabatPlusAlphaTable.Columns.Add("Key", typeof(string));
            this.QRAlphabatPlusAlphaTable.Columns.Add("Value", typeof(string));

            //data
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "0", "0" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "1", "1" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "2", "2" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "3", "3" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "4", "4" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "5", "5" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "6", "6" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "7", "7" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "8", "8" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "9", "9" });

            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "A", "10" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "B", "11" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "C", "12" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "D", "13" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "E", "14" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "F", "15" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "G", "16" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "H", "17" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "I", "18" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "J", "19" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "K", "20" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "L", "21" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "M", "22" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "N", "23" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "O", "24" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "P", "25" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "Q", "26" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "R", "27" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "S", "28" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "T", "29" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "U", "30" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "V", "31" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "W", "32" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "X", "33" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "Y", "34" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "Z", "35" });

            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { " ", "36" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "$", "37" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "%", "38" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "*", "39" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "+", "40" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "-", "41" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { ".", "42" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "/", "43" });
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { ":", "44" });

            //문자열이 홀수라면 마지막에 @ (마지막1개는 6bit 바이터리로 처리하기위함)
            this.QRAlphabatPlusAlphaTable.Rows.Add(new object[] { "@", "99" });
        } // end init_QRAlphabatPlusAlphaTable
        #endregion
    } // end class DataEncodingModeAlphabatPlusAlpha 
}
