using System.Text;
using System.IO;

namespace DataCore
{
    public static class StringUtils
    {
        #region Concatenation

        /// <summary> Concatenate the specified strings. </summary>
        /// <returns>The concatenated string.</returns>
        /// <param name="array">String array.</param>
        public static string Concat(params object[] array)
        {

            /*
             * Encapsulation of string builder to not interact with other
             * code and avoid unusal behaviour.        
            */

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
                stringBuilder.Append(array[i]);
            return stringBuilder.ToString();
        }

        /// <summary> Concatenates with the format. </summary>
        /// <returns>The format.</returns>
        /// <param name="format">Format.</param>
        /// <param name="array">Array.</param>
        public static string ConcatFormat(string format, params object[] array)
        {

            /*
             * Encapsulation of string builder to not interact with other
             * code and avoid unusal behaviour.        
            */

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(format, array);
            return stringBuilder.ToString();
        }

        #endregion
    }
}