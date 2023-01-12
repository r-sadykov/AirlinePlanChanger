using System.IO;

namespace AirlinePlanChanges_MailCenter.Utils
{
    public static class StreamUtils
    {
        /// <summary>
        /// Seeks the position of a string of data in the file.  The path...
        /// </summary>
        /// <param name="file"></param>
        /// <param name="searchString"></param>
        public static long Seek(string file, string searchString)
        {
            //open filestream to perform a seek
            using (FileStream fs =
                        File.OpenRead(file))
            {
                return Seek(fs, searchString);
            }
        }

        public static long Seek(string file, string searchString, int stringLen, int ignoreSym = 0, int ignoreLen=0)
        {
            using (FileStream fs =
                        File.OpenRead(file))
            {
                return Seek(fs, searchString, stringLen, ignoreSym, ignoreLen);
            }
        }

        private static long Seek(FileStream fs, string searchString, int stringLen, int ignoreSym, int ignoreLen)
        {
            char[] search = searchString.ToCharArray();
            if (search.Length != stringLen) return -1;
            long result = -1, position = 0, stored = -1,
            begin = fs.Position;
            int c;

            //read byte by byte
            while ((c = fs.ReadByte()) != -1)
            {
                //check if data in array matches
                if ((char)c == search[position])
                {
                    if (position >= ignoreSym && position <= (ignoreSym+ignoreLen))
                    {
                        position++;
                        continue;
                    }
                    // if charater matches first character of seek string, store it ...
                    if (stored == -1 && position > 0 && (char)c == search[0])
                    {
                        stored = fs.Position;
                    }

                    //check if we're done
                    if (position + 1 == search.Length)
                    {
                        //correct position for array lenth
                        result = fs.Position - search.Length;
                        //set position in stream
                        fs.Position = result;
                        break;
                    }

                    //advance position in the array
                    position++;
                }
                //no match, check if we have a stored position
                else if (stored > -1)
                {
                    //go to stored position + 1
                    fs.Position = stored + 1;
                    position = 1;
                    stored = -1; //reset stored position!
                }
                // no match, no stored position, reset array position and contin...
                else
                {
                    position = 0;
                }
            }

            //reset stream position if no match has been found
            if (result == -1)
            {
                fs.Position = begin;
            }

            return result;
        }

        /// <summary>
        /// Seeks the position of a string in the file stream. It will ad...
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="searchString"></param>

        public static long Seek(FileStream fs, string searchString)
        {
            char[] search = searchString.ToCharArray();
            long result = -1, position = 0, stored = -1,
            begin = fs.Position;
            int c;

            //read byte by byte
            while ((c = fs.ReadByte()) != -1)
            {
                //check if data in array matches
                if ((char)c == search[position])
                {
                    // if charater matches first character of seek string, store it ...
                    if (stored == -1 && position > 0 && (char)c == search[0])
                    {
                        stored = fs.Position;
                    }

                    //check if we're done
                    if (position + 1 == search.Length)
                    {
                        //correct position for array lenth
                        result = fs.Position - search.Length;
                        //set position in stream
                        fs.Position = result;
                        break;
                    }

                    //advance position in the array
                    position++;
                }
                //no match, check if we have a stored position
                else if (stored > -1)
                {
                    //go to stored position + 1
                    fs.Position = stored + 1;
                    position = 1;
                    stored = -1; //reset stored position!
                }
                // no match, no stored position, reset array position and contin...
                else
                {
                    position = 0;
                }
            }

            //reset stream position if no match has been found
            if (result == -1)
            {
                fs.Position = begin;
            }

            return result;
        }
    }
}
