using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2.TablaSimbolos
{
    class Ambitos
    {
        List<Simbolos> Simbolos;
        String Ambito;
        int nivel;
        public Ambitos(String Ambito, List<Simbolos> Simbolos,int nivel)
        {
            this.Ambito = Ambito;
            this.Simbolos = Simbolos;
            this.nivel = nivel;
        }

        public String getAmbito()
        {
            return Ambito;
        }
        public void setAmbito(String Ambito)
        {
            this.Ambito = Ambito;
        }

        public List<Simbolos> getSimbolos()
        {
            return Simbolos;
        }
        public void setSimbolos(List<Simbolos>  Simbolos)
        {
            this.Simbolos = Simbolos;
        }
        public int getNivel()
        {
            return nivel;
        }
        public void setNivel(int nivel)
        {
            this.nivel = nivel;
        }

    }
}
