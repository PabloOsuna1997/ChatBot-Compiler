using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto2
{
    public partial class Form1 : Form
    {
        NuevaPestana ins;
   
        public Form1()
        {
            InitializeComponent();
            ins = new NuevaPestana(tabControl1, richTextBox1);           
        }

       

        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ins.CrearNuevaPestaña();
        }

        private void cerrarPestañaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ins.CerrarPestaña();
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ins.Guardar();
        }

        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ins.GuardarComo();
        }

        private void ejecutarArchivoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ins.pintarpalabras();
            bool resultado = ins.analizar();

            if (resultado) Console.WriteLine("cadena correcta.");
            else Console.WriteLine("Cadena Incorrecta.");
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ins.CrearNuevaPestaña();
            ins.abrir();
        }
    }
}
