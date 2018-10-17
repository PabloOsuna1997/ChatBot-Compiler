using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace Proyecto2.Analisis
{
    class Analizar
    {
        //declaracion de listas para reportes de errore lexicos,sintacticos y semanticos.
        public static List<String> ErrSintacticos = new List<String>();
        public static List<String> ErrLexicos = new List<String>();
        public static List<String> ErrSemanticos = new List<String>();

        public static ParseTreeNode EjecucionAnalisis(String cadena)
        {
            Gramatica grama = new Gramatica();
            LanguageData lenguaje = new LanguageData(grama);
            Parser parse = new Parser(lenguaje);
            ParseTree arbol = parse.Parse(cadena);
            ParseTreeNode Root = arbol.Root;

            if (Root != null)
            {
                return Root;
            }
            else
            {
                return null;
            }
        }
    }
}
