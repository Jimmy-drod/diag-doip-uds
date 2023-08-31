using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diag_Doip_Uds.Lib.Utility_support.Utility
{
    public class Utils
    {
        public static string ConvertToHexString(byte _char_start, byte _char_count, List<byte> _input_buffer)
        {
            string hex_string = string.Empty;
            byte total_char_count = (byte)(_char_start + _char_count);

            for (byte char_start_count = _char_start; char_start_count < total_char_count; char_start_count++)
            {
                string vehicle_info_data_eid = string.Empty;
                int payload_byte = _input_buffer[char_start_count];
                if ((payload_byte <= 15U))
                {
                    // "0" appended in case of value upto 15/0xF
                    vehicle_info_data_eid += "0";
                }
                vehicle_info_data_eid += (payload_byte.ToString("X") + ":");
                hex_string += vehicle_info_data_eid;
            }
            // remove last ":" appended before
            return hex_string.Remove(hex_string.LastIndexOf(":"), 1); 
        }

        public static string ConvertToAsciiString(byte _char_start, byte _char_count,List<byte> _input_buffer)
        {
            string ascii_string = string.Empty;
            byte total_char_count = (byte)(_char_start + _char_count);

            for (byte char_start_count = _char_start; char_start_count < total_char_count; char_start_count++)
            {
                string vehicle_info_data_vin = string.Empty;
                vehicle_info_data_vin += _input_buffer[char_start_count];
                ascii_string += vehicle_info_data_vin;
            }
            return ascii_string;
        }

        public static void SerializeEIDGIDFromString(string _input_string, 
                                                 List<byte> _output_buffer, 
                                                       byte _total_size,
                                                       byte _substring_range)
        {

            for (var char_count = 0U; char_count < _total_size; char_count += _substring_range)
            {
                string input_string_new = _input_string.Substring((int)char_count, _substring_range);
                int get_byte = Convert.ToInt32(input_string_new, 16);
                _output_buffer.Add((byte)get_byte);
            }
        }

        public static void SerializeVINFromString(string _input_string, 
                                              List<byte> _output_buffer, 
                                                    byte _total_size,
                                                    byte _substring_range)
        {

            for (var char_count = 0U; char_count < _total_size; char_count += _substring_range)
            {
                string input_string_new = _input_string.Substring((int)char_count, _substring_range);
                //int get_byte = Convert.ToInt32(input_string_new, 16);
                byte get_byte = (byte)input_string_new[0];
                _output_buffer.Add((byte)get_byte);
            }
        }
    }
}
