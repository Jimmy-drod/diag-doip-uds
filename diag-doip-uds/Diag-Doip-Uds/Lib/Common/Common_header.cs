using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Common
{
    public class Common_header
    {
        /* Magic numbers */
        public const byte BYTE_POS_ZERO = 0x00;
        public const byte BYTE_POS_ONE = 0x01;
        public const byte BYTE_POS_TWO = 0x02;
        public const byte BYTE_POS_THREE = 0x03;
        public const byte BYTE_POS_FOUR = 0x04;
        public const byte BYTE_POS_FIVE = 0x05;
        public const byte BYTE_POS_SIX = 0x06;
        public const byte BYTE_POS_SEVEN = 0x07;

        public enum StdReturnType : byte
        {
            E_OK = 0x00,
            E_NOT_OK,
            E_BUSY
        }

        public class Pair<F, S>
        {
            public F First { get; set; }
            public S Second { get; set; }

            public Pair(F _first, S _second)
            {
                this.First = _first;
                this.Second = _second;
            }
        }
    }
}
