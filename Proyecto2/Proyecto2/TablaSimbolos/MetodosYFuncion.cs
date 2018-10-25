using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;


namespace Proyecto2.TablaSimbolos
{
    class MetodosYFuncion
    {
        String Nombre;
        String Tipo;
        ParseTreeNode Cuerpo;
        ParseTreeNode Return;
        ParseTreeNode Parametros;
        public MetodosYFuncion(String Nombre, String Tipo, ParseTreeNode Cuerpo, ParseTreeNode Parametros, ParseTreeNode Return)
        {
            this.Nombre = Nombre;
            this.Tipo = Tipo;
            this.Cuerpo = Cuerpo;
            this.Return = Return;
            this.Parametros = Parametros;
        }

        public String getNombre()
        {
            return Nombre;
        }
        public void setNombre(String Nombre)
        {
            this.Nombre = Nombre;
        }
        public String getTipo()
        {
            return Tipo;
        }
        public void setTipo(String Tipo)
        {
            this.Tipo = Tipo;
        }
        public ParseTreeNode getCuerpo()
        {
            return Cuerpo;
        }
        public void setCuerpo(ParseTreeNode Cuerpo)
        {
            this.Cuerpo = Cuerpo;
        }
        public ParseTreeNode getReturn()
        {
            return Return;
        }
        public void setReturn(ParseTreeNode Return)
        {
            this.Return = Return;
        }
        public ParseTreeNode getParametros()
        {
            return Parametros;
        }
        public void setRParametros(ParseTreeNode Parametros)
        {
            this.Parametros = Parametros;
        }
    }
}
