using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpQRPreStudySimpleEncode.Steps
{
    /// <summary>
    /// 2.2.4 종료 지시자
    /// 2.2.3에서 나온 결과에 0000을 추가한다. 
    /// 만약 2.2.3에서 이미 데이터 코드의 길이가 최대에 도달했다면 이 작업은 할 필요가 없다.
    /// </summary>
    public class DataMaximumLengthPerModel
    {
        /// <summary>
        /// 표1 의 데이터 테이블
        /// 버전 1~9의 데이터 코드와 오류 정정 코드의 갯수
        /// </summary>
        private DataTable QRMaximumLengthPerModelTable = new DataTable("QRMaximumLengthPerModelTable");

        /// <summary>
        /// 입력 데이터
        /// </summary>
        private string _inputData;

        /// <summary>
        /// 모드 지시자 값
        /// 숫자 모드 : 0001
        /// 알파벳(대문자만) + α 모드: 0010
        /// 8bit 모드: 0100
        /// KANJI 모드 : 1000
        /// </summary>
        private string _modeIndicator;

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
        /// <param name="inputData">데이터</param>
        /// <param name="modeIndicator">모드 지시자</param>
        public DataMaximumLengthPerModel(string inputData, string ver, string restore, string modeIndicator)
        {
            init_QRMaximumLengthPerModelTable();

            this._inputData = inputData;
            this._version = ver;
            this._errorRestoreLevel = restore;
            this._modeIndicator = modeIndicator;
        }

        /// <summary>
        /// I최대 길이 확인
        /// </summary>
        public string GetMaximumLengthPerModel
        {
            get { return MaximumLengthPerModel(); }
        }

        /// <summary>
        /// I최대 8bit 블락 크기
        /// </summary>
        public string GetMax8bitBlock
        {
            get { return Max8bitBlock(); }
        }

        /// <summary>
        /// 데이터 코드의 최대 크기 반환
        /// 만약 데이터 코드가 표1의 최대 개수만큼 차지 못했다면 
        /// "11101100"과 "00010001"을 번갈아 가면서 최대 개수까지 추가해준다.
        /// </summary>
        /// <returns></returns>
        private string Max8bitBlock()
        {
            DataRow[] rows;
            rows = this.QRMaximumLengthPerModelTable.Select(
                string.Format("Version = '{0}' AND Restore = '{1}'", _version, _errorRestoreLevel)
                );

            return rows[0]["DataCodeCount"].ToString(); ;
        }

        /// <summary>
        /// 최대 길이 확인
        /// 최대 길이이면 빈값을 반환
        /// 아니라면 0000을 반환
        /// 2.2.3에서 나온 결과에 0000을 추가한다. 
        /// 만약 2.2.3에서 이미 데이터 코드의 길이가 최대에 도달했다면 이 작업은 할 필요가 없다.
        /// </summary>
        /// <returns></returns>
        private string MaximumLengthPerModel()
        {
            string resultTailString = string.Empty;

            DataRow[] rows;
            rows = this.QRMaximumLengthPerModelTable.Select(
                string.Format("Version = '{0}' AND Restore = '{1}'", _version, _errorRestoreLevel)
                );

            string _fieldName = string.Empty;
            //숫자 모드 : 0001
            //알파벳(대문자만) + α 모드: 0010
            //8bit 모드: 0100
            //KANJI 모드 : 1000
            switch (_modeIndicator)
            {
                case "0001":
                    _fieldName = "Numberic";
                    break;
                case "0010":
                    _fieldName = "Alphanumeric";
                    break;
                case "0100":
                    _fieldName = "Eightbit";
                    break;
                case "1000":
                    _fieldName = "Alphanumeric"; //KANJI 모드는 임시로 이거로..
                    break;
            }

            int _maximumLength = 0;
            _maximumLength = int.Parse(rows[0][_fieldName].ToString());

            //만약 2.2.3에서 이미 데이터 코드의 길이가 최대에 도달했다면 이 작업은 할 필요가 없다.
            if (_inputData.Length < _maximumLength)
            {
                resultTailString = "0000";
            }

            return resultTailString;
        } // end MaximumLengthPerModel

        #region 표1. 버전 1~9의 데이터 코드와 오류 정정 코드의 갯수
        /// <summary>
        /// 표1. 버전 1~9의 데이터 코드와 오류 정정 코드의 갯수
        /// </summary>
        private void init_QRMaximumLengthPerModelTable()
        {
            //대소문자 사용 해야하나???
            this.QRMaximumLengthPerModelTable.CaseSensitive = false;

            //항목
            //버전/오류 정정 단계/데이터 코드의 갯수/오류 정정 코드의 갯수/숫자/알파벳+α/8bit
            // numeric / alphanumeric / eightbit
            this.QRMaximumLengthPerModelTable.Columns.Add("Version", typeof(string));
            this.QRMaximumLengthPerModelTable.Columns.Add("Restore", typeof(string));
            this.QRMaximumLengthPerModelTable.Columns.Add("DataCodeCount", typeof(string));
            this.QRMaximumLengthPerModelTable.Columns.Add("ErrorRestoreCount", typeof(string));
            this.QRMaximumLengthPerModelTable.Columns.Add("Numberic", typeof(string));
            this.QRMaximumLengthPerModelTable.Columns.Add("Alphanumeric", typeof(string));
            this.QRMaximumLengthPerModelTable.Columns.Add("Eightbit", typeof(string));

            //data
            //this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "A", "B", "C", "D", "E", "F", "G" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "1", "L", "19", "7", "41", "25", "17" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "1", "M", "16", "10", "34", "20", "14" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "1", "Q", "13", "13", "27", "16", "11" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "1", "H", "9", "17", "17", "10", "7" });

            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "2", "L", "34", "10", "77", "47", "32" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "2", "M", "28", "16", "63", "38", "26" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "2", "Q", "22", "22", "48", "29", "20" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "2", "H", "16", "28", "34", "20", "14" });

            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "3", "L", "55", "15", "127", "77", "53" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "3", "M", "44", "26", "101", "61", "42" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "3", "Q", "34", "36", "77", "47", "32" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "3", "H", "26", "44", "58", "35", "24" });

            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "4", "L", "80", "20", "187", "114", "78" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "4", "M", "64", "36", "149", "90", "62" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "4", "Q", "48", "52", "111", "67", "46" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "4", "H", "36", "64", "82", "50", "34" });

            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "5", "L", "108", "26", "255", "154", "106" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "5", "M", "86", "48", "202", "122", "84" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "5", "Q", "62", "72", "144", "87", "60" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "5", "H", "46", "88", "106", "64", "44" });

            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "6", "L", "136", "36", "322", "195", "134" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "6", "M", "108", "64", "255", "154", "106" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "6", "Q", "76", "96", "175", "108", "74" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "6", "H", "60", "112", "139", "84", "58" });

            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "7", "L", "156", "40", "370", "224", "154" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "7", "M", "124", "72", "293", "178", "122" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "7", "Q", "88", "108", "207", "125", "86" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "7", "H", "66", "130", "154", "93", "64" });

            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "8", "L", "194", "48", "461", "279", "192" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "8", "M", "154", "88", "365", "221", "152" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "8", "Q", "110", "132", "259", "157", "108" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "8", "H", "86", "156", "202", "122", "84" });

            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "9", "L", "232", "60", "552", "335", "230" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "9", "M", "182", "110", "432", "262", "180" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "9", "Q", "132", "160", "312", "189", "130" });
            this.QRMaximumLengthPerModelTable.Rows.Add(new object[] { "9", "H", "100", "192", "235", "143", "98" });

            //... 여기까지만 일단 입력 ㄷㄷ

        } // end init_QRMaximumLengthPerModelTable
        #endregion
    } // end class DataMaximumLengthPerModel
}
