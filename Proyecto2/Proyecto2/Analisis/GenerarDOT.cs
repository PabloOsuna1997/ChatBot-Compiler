using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace Proyecto2.Analisis
{
    class GenerarDOT
    {
        static int contador;
        static String dot;

        public static String GeneracionDot(ParseTreeNode Raiz)
        {
            dot = "digraph Arbol{";
            dot += "nodo[label = \"" + Salida(Raiz.ToString()) + "\"];\n";
            contador = 1;
            Recorrido("nodo0", Raiz);
            dot += "}";

            return dot;
        }

        public static void Recorrido(String Padre, ParseTreeNode hijos)
        {
            foreach (ParseTreeNode hijo in hijos.ChildNodes)
            {
                String NodoHijo = "nodo" + contador.ToString();
                dot += NodoHijo + "[label = \"" + Salida(hijo.ToString()) + "\"];\n";
                dot += Padre + "->" + NodoHijo + ";\n";
                contador++;
                Recorrido(NodoHijo, hijo);
            }
        }

        public static String Salida(String Cadena)
        {
            Cadena = Cadena.Replace("\\", "\\\\");
            Cadena = Cadena.Replace("\"", "\\\"");

            return Cadena;
        }
    }
}
