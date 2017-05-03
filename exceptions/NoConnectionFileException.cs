using System;
using System.Collections.Generic;
using System.Text;

namespace bvmfscrapper.exceptions
{
    public class NoConnectionFileException : Exception
    {
        public NoConnectionFileException()
            :base("Arquivo connection.string não encontrado")
        {

        } 
    }
}
