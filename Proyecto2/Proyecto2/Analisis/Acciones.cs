using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace Proyecto2.Analisis
{
    class Acciones
    {
        public static void RealizarAccionesAcciones(ParseTreeNode Raiz) {

            EjecucionAcciones(Raiz);
        }

        public static void EjecucionAcciones(ParseTreeNode Nodo){

            switch (Nodo.Term.Name.ToString()) {


            }
        }
    }
}
