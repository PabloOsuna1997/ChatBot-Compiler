using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proyecto2.Analisis;
using Irony.Ast;
using Irony.Parsing;
using System.IO;

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
                generardot(Root);           //si la cadena es correcta mandaremos a generar el dot del arbol para crear la imagen.
                return Root;
            }
            else
            {
                return null;
            }
        }

        private static void generardot(ParseTreeNode Raiz)
        {
            String dot = GenerarDOT.GeneracionDot(Raiz);

            try
            {
                StreamWriter sw = new StreamWriter("C:\\Users\\asddd\\OneDrive\\Escritorio\\DOT.dot");
                sw.WriteLine(dot);
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("No se genero dot: " + e);
            }

        }
    }
}
