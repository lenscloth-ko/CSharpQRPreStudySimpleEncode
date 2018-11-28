using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CSharpQRStudySimpleEncode.Steps
{
    /// <summary>
    /// 2.3 오류 정정 코드 계산
    /// QR코드의 오류 정정에는 Reed-solomon 알고리즘이 사용된다.
    /// </summary>
    public class ReedSolomonBlock
    {
        /// <summary>
        /// 표1 의 데이터 테이블
        /// 버전 1~9의 데이터 코드와 오류 정정 코드의 갯수
        /// </summary>
        private DataTable QRReedSolomBlockTable = new DataTable("QRReedSolomBlockTable");

        /// <summary>
        /// 버전
        /// </summary>
        private string _version;

        /// <summary>
        /// 오류 정정 단계
        /// </summary>
        private string _errorRestoreLevel;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="ver">버전</param>
        /// <param name="restore">오류 정정 단계</param>
        public ReedSolomonBlock(string ver, string restore)
        {
            init_QRReedSolomBlockTable();

            this._version = ver;
            this._errorRestoreLevel = restore;
        }

        /// <summary>
        /// i오류 정정 코드의 갯수 반환
        /// </summary>
        public int GetErrorRepairCodeCount
        {
            get { return ErrorRepairCodeCount(); }
        }

        /// <summary>
        /// iRS 블럭 GFDimension 값 반환
        /// </summary>
        public int GetGFDimension
        {
            get { return GFDimension(); }
        }

        /// <summary>
        /// iRS 블럭 RSLength 값 반환
        /// </summary>
        public int GetRSLength
        {
            get { return RSLength(); }
        }

        /// <summary>
        /// iRS 블럭 ParityLength 값 반환
        /// </summary>
        public int GetParityLength
        {
            get { return ParityLength(); }
        }

        /// <summary>
        /// 오류 정정 코드의 갯수 반환
        /// </summary>
        /// <returns>오류 정정 코드의 갯수</returns>
        private int ErrorRepairCodeCount()
        {
            DataRow[] rows;
            rows = this.QRReedSolomBlockTable.Select(
                string.Format("Version = '{0}' AND Restore = '{1}'", _version, _errorRestoreLevel)
                );

            return int.Parse(rows[0]["ErrorRestoreCount"].ToString());
        }

        /// <summary>
        /// RS 블럭 GFDimension 값 반환
        /// </summary>
        /// <returns></returns>
        private int GFDimension()
        {
            DataRow[] rows;
            rows = this.QRReedSolomBlockTable.Select(
                string.Format("Version = '{0}' AND Restore = '{1}'", _version, _errorRestoreLevel)
                );

            return int.Parse(rows[0]["GFDimension"].ToString());
        }

        /// <summary>
        /// RS 블럭 RSLength 값 반환
        /// </summary>
        /// <returns></returns>
        private int RSLength()
        {
            DataRow[] rows;
            rows = this.QRReedSolomBlockTable.Select(
                string.Format("Version = '{0}' AND Restore = '{1}'", _version, _errorRestoreLevel)
                );

            return int.Parse(rows[0]["RSLength"].ToString());
        }

        /// <summary>
        /// RS 블럭 ParityLength 값 반환
        /// </summary>
        /// <returns></returns>
        private int ParityLength()
        {
            DataRow[] rows;
            rows = this.QRReedSolomBlockTable.Select(
                string.Format("Version = '{0}' AND Restore = '{1}'", _version, _errorRestoreLevel)
                );

            return int.Parse(rows[0]["ParityLength"].ToString());
        }

        #region 표1. 버전 1~9의 데이터 코드와 오류 정정 코드의 갯수
        /// <summary>
        /// 표1. 버전 1~9의 데이터 코드와 오류 정정 코드의 갯수
        /// </summary>
        private void init_QRReedSolomBlockTable()
        {
            //대소문자 사용 해야하나???
            this.QRReedSolomBlockTable.CaseSensitive = false;

            //항목
            //버전/오류 정정 단계/데이터 코드의 갯수/오류 정정 코드의 갯수/숫자/알파벳+α/8bit
            // iReedSolomon(int GFDimension, int RSLength, int ParityLength)
            this.QRReedSolomBlockTable.Columns.Add("Version", typeof(string));
            this.QRReedSolomBlockTable.Columns.Add("Restore", typeof(string));
            this.QRReedSolomBlockTable.Columns.Add("DataCodeCount", typeof(string));
            this.QRReedSolomBlockTable.Columns.Add("ErrorRestoreCount", typeof(string));
            this.QRReedSolomBlockTable.Columns.Add("RSBlockCount", typeof(string));
            this.QRReedSolomBlockTable.Columns.Add("GFDimension", typeof(string));
            this.QRReedSolomBlockTable.Columns.Add("RSLength", typeof(string));
            this.QRReedSolomBlockTable.Columns.Add("ParityLength", typeof(string));

            //data
            //this.QRReedSolomBlockTable.Rows.Add(new object[] { "A", "B", "C", "D", "E", "F", "G" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "1", "L", "19", "7", "1", "26", "19", "2" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "1", "M", "16", "10", "1", "26", "16", "4" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "1", "Q", "13", "13", "1", "26", "13", "6" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "1", "H", "9", "17", "1", "26", "9", "8" });

            this.QRReedSolomBlockTable.Rows.Add(new object[] { "2", "L", "34", "10", "1", "44", "34", "4" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "2", "M", "28", "16", "1", "44", "28", "8" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "2", "Q", "22", "22", "1", "44", "22", "11" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "2", "H", "16", "28", "1", "44", "16", "14" });

            this.QRReedSolomBlockTable.Rows.Add(new object[] { "3", "L", "55", "15", "1", "70", "55", "7" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "3", "M", "44", "26", "1", "70", "44", "13" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "3", "Q", "34", "36", "2", "35", "17", "9" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "3", "H", "26", "44", "2", "35", "13", "11" });

            this.QRReedSolomBlockTable.Rows.Add(new object[] { "4", "L", "80", "20", "1", "100", "80", "10" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "4", "M", "64", "36", "2", "50", "32", "9" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "4", "Q", "48", "52", "2", "50", "24", "13" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "4", "H", "36", "64", "4", "25", "9", "8" });

            this.QRReedSolomBlockTable.Rows.Add(new object[] { "5", "L", "108", "26", "1", "134", "108", "13" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "5", "M", "86", "48", "2", "67", "43", "12" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "5", "Q", "62", "72", "2", "33", "15", "9" });
            //this.QRReedSolomBlockTable.Rows.Add(new object[] { "5", "Q", "62", "72", "2", "34", "16", "9" });
            this.QRReedSolomBlockTable.Rows.Add(new object[] { "5", "H", "46", "88", "2", "33", "11", "11" });
            //this.QRReedSolomBlockTable.Rows.Add(new object[] { "5", "H", "46", "88", "2", "34", "12", "11" });

            //... 여기까지만 일단 입력 ㄷㄷ

            //for (int i = 0; i <= 255 ; i++)
            //{
            //    Console.WriteLine(i + "/" + Math.Pow(2,i) + "/" + Convert.ToInt32(Math.Pow(2, i)));
            //}

        } // end init_QRReedSolomBlockTable
        #endregion


    }
} // end namespace
