using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DESAlgorithm
{
    public static class UnicodeUtil
    {
        // 8바이트 데이터 배열을 유니코드 문자열로 변환하는 함수
        public static string ToUnicodeString(this long[] data)
        {
            StringBuilder sb = new StringBuilder();

            // long (8바이트) 배열에서 원소를 하나씩 꺼내서 유니코드 문자열로 변환한다.
            foreach (var b in data)
            {
                sb.Append(Encoding.Unicode.GetString(BitConverter.GetBytes(b)));
            }

            // 변환한 유니코드 문자열 반환
            return sb.ToString();
        }
    }
    public class DESAlgorithm
    {
        #region 설정
        public char paddingChar { get; set; } = ' '; // 블럭의 남은 공간을 패딩할 문자
        #endregion

        #region 암호화 관련 Table

        /// <summary>
        /// 초기 순열 테이블
        /// </summary>
        private int[] ipTable = {
            58, 50, 42, 34, 26, 18, 10, 2,
            60, 52, 44, 36, 28, 20, 12, 4,
            62, 54, 46, 38, 30, 22, 14, 6,
            64, 56, 48, 40, 32, 24, 16, 8,
            57, 49, 41, 33, 25, 17, 9, 1,
            59, 51, 43, 35, 27, 19, 11, 3,
            61, 53, 45, 37, 29, 21, 13, 5,
            63, 55, 47, 39, 31, 23, 15, 7
        };

        /// <summary>
        /// 역 초기 순열 테이블
        /// </summary>
        private int[] fpTable = {
            40, 8, 48, 16, 56, 24, 64, 32,
            39, 7, 47, 15, 55, 23, 63, 31,
            38, 6, 46, 14, 54, 22, 62, 30,
            37, 5, 45, 13, 53, 21, 61, 29,
            36, 4, 44, 12, 52, 20, 60, 28,
            35, 3, 43, 11, 51, 19, 59, 27,
            34, 2, 42, 10, 50, 18, 58, 26,
            33, 1, 41, 9, 49, 17, 57, 25
        };

        /// <summary>
        /// 확장 함수 테이블
        /// </summary>
        private int[] expTable = {
            32, 1, 2, 3, 4, 5,
            4, 5, 6, 7, 8, 9,
            8, 9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32, 1,
        };

        /// <summary>
        /// 순열 함수 테이블
        /// </summary>
        private int[] pTable = {
            16, 7, 20, 21, 29, 12, 28, 17,
            1, 15, 23, 26, 5, 18, 31, 10,
            2, 8, 24, 14, 32, 27, 3, 9,
            19, 13, 30, 6, 22, 11, 4, 25
        };

        /// <summary>
        /// 순열 선택1 테이블
        /// </summary>
        private int[] pc1Table = {
            57, 49, 41, 33, 25, 17, 9,
            1, 58, 50, 42, 34, 26, 18,
            10, 2, 59, 51, 43, 35, 27,
            19, 11, 3, 60, 52, 44, 36,
            63, 55, 47, 39, 31, 23, 15,
            7, 62, 54, 46, 38, 30, 22,
            14, 6, 61, 53, 45, 37, 29,
            21, 13, 5, 28, 20, 12, 4
        };

        /// <summary>
        /// 순열 선택2 테이블
        /// </summary>
        private int[] pc2Table = {
            14, 17, 11, 24, 1, 5, 3, 28,
            15, 6, 21, 10, 23, 19, 12, 4,
            26, 8, 16, 7, 27, 20, 13, 2,
            41, 52, 31, 37, 47, 55, 30, 40,
            51, 45, 33, 48, 44, 49, 39, 56,
            34, 53, 46, 42, 50, 36, 29, 32
        };

        /// <summary>
        /// 좌측 이동 스케줄
        /// </summary>
        private int[] leftShiftTable = {
            1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1
        };

        /// <summary>
        /// S-Box 테이블
        /// </summary>
        private int[][] sboxTable = {
            new int[]
            {
                14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7,
                0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8,
                4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0,
                15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13
            },
            new int[]
            {
                15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10,
                3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5,
                0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15,
                13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9
            },
            new int[]
            {
                10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8,
                13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1,
                13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7,
                1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12
            },
            new int[]
            {
                7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15,
                13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9,
                10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4,
                3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14
            },
            new int[]
            {
                2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9,
                14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6,
                4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14,
                11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3
            },
            new int[]
            {
                12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11,
                10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8,
                 9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6,
                 4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13
            },
            new int[]
            {
                4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1,
                13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6,
                1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2,
                6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12
            },
            new int[]
            {
                13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7,
                1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2,
                7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8,
                2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11
            }
        };
        #endregion

        #region 키 생성 함수
        /// <summary>
        /// 순열 선택 함수 1
        /// </summary>
        /// <param name="key64"></param>
        /// <returns></returns>
        private long PermutedChoice1(long key64)
        {
            long result = 0;

            for (int i=0; i<56; i++)
            { // 순열 선택 테이블을 참조하여 64비트로 입력된 키를 56비트로 변환
                result |= ((((key64 >> (64 - pc1Table[i])) & 0x1) << (55 - i)));
            }

            return result;
        }

        /// <summary>
        /// 좌측 순환 시프트
        /// </summary>
        /// <param name="key56">56비트 키</param>
        /// <param name="shiftCount">시프트 횟수</param>
        /// <returns></returns>
        private long ShiftLeft(long key56, int shiftCount)
        {
            int left = (int)((key56 >> 28) & 0xfffffff); // 좌측 28비트
            int right = (int)(key56 & 0xfffffff); // 우측 28비트

            // 시프트 횟수만큼 좌측 시프팅
            left <<= shiftCount; 
            right <<= shiftCount;

            int cout = (left & 0x30000000) >> 28; // 좌측 28비트에서 초과된 비트
            left = (left & 0x0fffffff) | cout; // 초과된 비트를 다시 우측에 더함
            cout = (right & 0x30000000) >> 28; // 우측 28비트에서 초과된 비트
            right = (right & 0x0fffffff) | cout; // 초과된 비트를 다시 우측에 더함

            return ((long)left << 28) | (long)right; // 좌측 28비트 + 우측 28비트 = 56비트
        }

        /// <summary>
        /// 순열 선택 함수 2
        /// </summary>
        /// <param name="key56"></param>
        /// <returns></returns>
        private long PermutedChoice2(long key56)
        {
            long result = 0;

            for(int i=0; i<48; i++)
            { //순열 선택2 테이블을 참조하여 56비트 키를 48비트 서브키로 변환
                result |= (((key56 >> (56 - pc2Table[i])) & 0x1) << (47 - i));
            }

            return result;
        } // PC-2

        public long[] GenerateSubkeys(long key64)
        {
            long[] subkeys = new long[16]; // 서브키 배열

            long key56 = PermutedChoice1(key64); // 먼저, 64비트 키에 대해 순열 선택1 함수를 실행한다.

            for (int i = 0; i < 16; i++)
            { // 16번 만큼 좌측 시프트와 순열 선택2 함수를 반복하여 각 라운드에서 사용할 서브키를 생성한다.
                key56 = ShiftLeft(key56, leftShiftTable[i]);
                subkeys[i] = PermutedChoice2(key56);
            }

            // 생성된 서브키 배열을 반환한다.
            return subkeys;
        }

        #endregion

        /// <summary>
        /// 암호화/복호화를 위한 초기 순열 함수
        /// </summary>
        /// <param name="plainText64"></param>
        /// <returns></returns>
        private long InitialPermutation(long plainText64)
        {
            long result = 0;

            for(int i=0; i<64; i++) 
            { // 초기순열 테이블을 참조하여 비트 재배열
                result |= (((plainText64 >> (64 - ipTable[i])) & 0x1) << (63 - i));
            }

            return result; // 재배열된 평문 반환
        }

        /// <summary>
        /// 암호화/복호화를 위한 역 초기 순열 함수
        /// </summary>
        /// <param name="plainText64"></param>
        /// <returns></returns>
        private long FinalPermutation(long plainText64)
        {
            long result = 0;

            for (int i = 0; i < 64; i++)
            { // 역 초기순열 테이블을 참조하여 비트 재배열
                result |= (((plainText64 >> (64 - fpTable[i])) & 0x1) << (63 - i));
            }

            return result;
        }

        /// <summary>
        /// 암호화 함수 f
        /// </summary>
        /// <param name="plainText32"></param>
        /// <param name="key48"></param>
        /// <returns></returns>
        private uint EncryptFunction(uint plainText32, long key48)
        {
            var l1 = Expansion(plainText32); // 확장 함수(32비트 -> 48비트)
            var l2 = Xor(l1, key48); // 확장된 데이터와 48비트 서브키를 Exclusive-OR 연산하는 함수
            var l3 = SubstituteChoice(l2); // 치환 선택 함수
            var l4 = Permutation(l3); // 순열 함수
            return l4; // 함수 f의 결과를 반환
        }

        /// <summary>
        /// 라운드 함수
        /// </summary>
        /// <param name="plainText64"></param>
        /// <param name="subkeys"></param>
        /// <param name="roundNumber"></param>
        /// <returns></returns>
        private long Round(long plainText64, long subkey)
        {
            // 64비트 데이터를 좌측 32비트, 우측 32비트로 나눈다
            uint left32 = (uint)(plainText64 >> 32);
            uint right32 = (uint)(plainText64 & 0xffffffff);

            long result = (long)right32 << 32; // 결과의 상위 32비트는 하위 32비트가 그대로 들어간다.

            // 암호화 함수 f를 거친 우측 32비트를 다시 좌측 32비트와 Exclusive-OR 연산을 한다.
            right32 = (uint)Xor(left32, EncryptFunction(right32, subkey));

            result |= (long)right32; // 연산 결과 32비트는 결과의 하위 32비트가 된다.

            return result; // 라운드를 한 번 수행한 결과를 반환한다.
        }

        /// <summary>
        /// Exclusive-OR 함수
        /// </summary>
        /// <param name="left">왼쪽 피연산자</param>
        /// <param name="right">오른쪽 피연산자</param>
        /// <param name="bitCount">비트 수</param>
        /// <returns></returns>
        private long Xor(long left, long right)
        {
            return left ^ right;
        }

        /// <summary>
        /// 암호화 함수 내의 확장 함수
        /// </summary>
        /// <param name="plainText32"></param>
        /// <returns></returns>
        private long Expansion(long plainText32)
        {
            long result = 0;

            for(int i=0; i<48; i++)
            { // 확장 테이블을 참조하여 32비트 데이터를 48비트로 확장한다.
                result |= (((plainText32 >> (32 - expTable[i])) & 0x1) << (47 - i));
            }

            return result;
        }

        /// <summary>
        /// 암호화 함수 내의 치환 선택 함수
        /// </summary>
        /// <param name="plainText48"></param>
        /// <returns></returns>
        private uint SubstituteChoice(long plainText48)
        {
            uint result = 0;

            for (int i=0; i<8; i++)
            {
                int temp = (int)(plainText48 >> (6 * i)) & 0x3f; // 48비트에서 6비트씩 추출한다.
                int row = ((temp & 0x20) >> 4) | (temp & 0x1); // 0, 5번 비트는 S-Box에서의 행이 된다. (0~3)
                int col = (temp >> 1) & 0xf; // 1 ~ 4번 비트는 S-Box에서의 열이 된다. (0~15)

                result |= (uint)sboxTable[7 - i][16 * row + col] << (4 * i); // 6비트씩 8개의 S-Box를 통과시킨다.
            }

            return result;
        }

        /// <summary>
        /// 암호화 함수 내의 순열 함수
        /// </summary>
        /// <param name="plainText32"></param>
        /// <returns></returns>
        private uint Permutation(uint plainText32)
        {
            uint result = 0;

            for(int i=0; i<32; i++)
            { // 순열 테이블을 참조하여 비트열을 뒤섞는다.
                result |= ((plainText32 >> (32 - pTable[i])) & 0x1) << (31 - i);
            }

            return result;
        }


        /// <summary>
        /// 좌측 32비트와 우측 32비트 교환
        /// </summary>
        /// <param name="plainText64"></param>
        /// <returns></returns>
        long SwapBytes(long plainText64)
        {
            long left = (plainText64 >> 32) & 0xffffffff;
            long right = plainText64 & 0xffffffff;
            
            return (right << 32) | left; // 좌측 32비트와 우측 32비트의 위치를 바꾼다.
        }

        /// <summary>
        /// 블록 단위(8Byte) 암호화 함수
        /// </summary>
        /// <param name="plainText64"></param>
        /// <param name="subkeys"></param>
        /// <returns></returns>
        private long EncryptUnit(long plainText64, long[] subkeys)
        { 
            plainText64 = InitialPermutation(plainText64); // 먼저, 평문에 대해 초기 순열을 행한다.

            for (int round = 0; round < 16; round++)
            { // 라운드 함수를 16번 실행한다.
                plainText64 = Round(plainText64, subkeys[round]);
            }

            plainText64 = SwapBytes(plainText64); // 좌측 32비트와 우측 32비트를 교환한다.
            return FinalPermutation(plainText64); // 역 초기 순열을 수행한 후 반환한다.
        }

        /// <summary>
        /// 블록 단위(8Byte) 복호화 함수
        /// </summary>
        /// <param name="cipherText64"></param>
        /// <param name="subkeys"></param>
        /// <returns></returns>
        private long DecryptUnit(long cipherText64, long[] subkeys)
        {
            cipherText64 = InitialPermutation(cipherText64); // 먼저, 암호문에 대해 초기 순열을 행한다.

            for (int round = 0; round <16; round++)
            { // 라운드 함수를 16번 실행한다.
                cipherText64 = Round(cipherText64, subkeys[15 - round]);
            }

            cipherText64 = SwapBytes(cipherText64); // 좌측 32비트와 우측 32비트를 교환한다.
            return FinalPermutation(cipherText64); // 역 초기 순열을 수행한 후 반환한다.
        }

        /// <summary>
        /// 문장 전체 암호화 함수
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="subkeys"></param>
        /// <returns></returns>
        public long[] Encrypt(string plainText, long[] subkeys)
        {
            // 유니코드 인코딩 방식에서의 문자열 길이를 구함
            int length = Encoding.Unicode.GetBytes(plainText).Length; 
            if (length % 8 > 0) // 문자열 길이가 8로 나누어 떨어지지 않으면, 
            {
                int remain = 8 - (length % 8); // 부족한 문자 수를 구한다.
                for (int i = 0; i < remain; i++)
                { // 부족한 문자 수 만큼 설정한 패딩 문자로 채운다.
                    plainText += paddingChar; 
                    length++;
                }
            }

            // 암호화를 하기 위해 입력한 문장을 유니코드 형식의 바이트 배열로 변환한다.
            byte[] byteData = Encoding.Unicode.GetBytes(plainText).ToArray(); 
            List<long> encryptedData = new List<long>();  
            for (int i = 0; i < length / 8; i++)
            {
                byte[] temp = new byte[8];
                Buffer.BlockCopy(byteData, 8 * i, temp, 0, 8); // 배열에서 8바이트씩 블럭 단위로 나누고
                encryptedData.Add(EncryptUnit(BitConverter.ToInt64(temp, 0), subkeys)); // 블럭 단위로 암호화 후 결과를 리스트에 저장한다.
            }

            return encryptedData.ToArray(); // 암호문 블럭 배열을 반환한다.
        }

        /// <summary>
        /// 문장 전체 복호화 함수
        /// </summary>
        /// <param name="EncryptedData"></param>
        /// <param name="subkeys"></param>
        /// <returns></returns>
        public long[] Decrypt(long[] EncryptedData, long[] subkeys)
        { 
            List<long> restoredText = new List<long>();
            foreach(long enc in EncryptedData) // 각각의 암호문 블럭에 대하여
            { // 서브키를 갖고 블럭 단위로 복호화 후 리스트에 저장한다. 
                restoredText.Add(DecryptUnit(enc, subkeys));
            }

            return restoredText.ToArray(); // 복호화된 문장 리스트를 반환한다.
        }
    }
}
