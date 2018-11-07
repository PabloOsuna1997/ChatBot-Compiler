using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Irony.Parsing;
using Irony.Ast;
using Proyecto2.Analisis;

namespace Proyecto2
{
    class NuevaPestana
    {
        TabControl ControladorPesta;
        RichTextBox Consola;
        TabPage Pestana;
        RichTextBox Area;
        SaveFileDialog guardar;
        RichTextBox richTextBox1;

        //arreglos que pintaran palabras cuando se haga match
        string[] Reservadas = new string[] { "Void","Int","Float","Boolean","Char","String","Double","For","If","Else","Do","While","Switch","Case","Default","Break","CompareTo","GetUser","Print","Return","Import"};
        string[] Numeros = new string[] { "1","2","3","4","5","6","7","8","9","0"};
        int posicion = 0;

        public NuevaPestana(TabControl ControladorPesta, RichTextBox Consola)
        {
            this.ControladorPesta = ControladorPesta;
            this.Consola = Consola;
            richTextBox1 = new RichTextBox();
        }

        public void CrearNuevaPestaña()
        {
            Font fuente = new Font("Consola",16);
            Pestana = new TabPage("Pestaña");
            Area = new RichTextBox();
            Area.SetBounds(1, 1, 715, 335);
            Area.AcceptsTab = true;
            Area.Font = fuente;
            Pestana.Controls.Add(Area);

            ControladorPesta.Controls.Add(Pestana);
            ControladorPesta.SelectedIndex = ControladorPesta.SelectedIndex + 1;           

        }

        public TabPage PestañaSeleccionda()
        {
            return ControladorPesta.SelectedTab;
        }

        public void CerrarPestaña()
        {
            ControladorPesta.TabPages.Remove(PestañaSeleccionda());
        }

        public void pintarpalabras()
        {
            ejecucion();
            richTextBox1 = (RichTextBox)PestañaSeleccionda().GetNextControl(PestañaSeleccionda(), true);
            richTextBox1.SelectionStart = richTextBox1.Text.Length;

            richTextBox1.TextChanged += (ob, ev) =>
            {
                posicion = richTextBox1.SelectionStart;
                ejecucion();
            };
        }

        private void ejecucion()
        {
            richTextBox1.Select(0, richTextBox1.Text.Length);
            richTextBox1.SelectionColor = Color.Black;
            richTextBox1.Select(posicion, 0);

            string[] texto = richTextBox1.Text.Trim().Split(' ');
            int inicio = 0;

            foreach (string x in texto)
            {
                foreach (string y in Reservadas)
                {
                    if (x.Length != 0)
                    {
                        if (x.Trim().Equals(y))
                        {
                            inicio = richTextBox1.Text.IndexOf(x, inicio);
                            richTextBox1.Select(inicio, x.Length);
                            richTextBox1.SelectionColor = Color.Blue;
                            richTextBox1.Select(posicion, 0);
                            inicio = inicio + 1;
                        }
                    }
                }
               /* foreach (string y in Numeros)
                {
                    if (x.Length != 0)
                    {
                        if (x.Trim().Equals(y))
                        {
                            inicio = richTextBox1.Text.IndexOf(x, inicio);
                            richTextBox1.Select(inicio, x.Length);
                            richTextBox1.SelectionColor = Color.Purple;
                            richTextBox1.Select(posicion, 0);
                            inicio = inicio + 1;
                        }
                    }
                }*/
            }
        }

        public bool analizar()
        {
            ParseTreeNode Root;
            //se captura la pestaña seleccionada y el siguiente componente que sera el textarea, luego se parseara a string
            String TextoAanalizar = PestañaSeleccionda().GetNextControl(PestañaSeleccionda(), true).Text;

            if (!TextoAanalizar.Equals(""))
            {
                Consola.Clear();
                Root = Analizar.EjecucionAnalisis(TextoAanalizar);        //llamo al metodo analizar de la clase analizar por ende esta public y estatica

                if (Root != null)
                {
                    Acciones.RealizarAccionesAcciones(Root);        //la cadena es correcta mandamos a ejecutar las acciones
                    List<String> impresiones = Analisis.Acciones.Impresiones; //despues las impresiones en consola de los print
                    for (int i = 0; i < impresiones.Count; i++)
                    {
                        Consola.AppendText(impresiones[i]+"\n");
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Area Vacia", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return false;
            }
        }

        #region ARCHIVO GUARDAR,GUARDAR COMO, ABRIR

        String pathEntrada = "";           //path de archivo cargado.
        String PathGuardado = "";          //path de archivo si ya se guardo alguna ves.

        public void abrir()
        {
            try
            {
                OpenFileDialog abrir = new OpenFileDialog();
                abrir.Filter = "All Files (*.cbc)|*.cbc"; // tipos de formatos
                if (abrir.ShowDialog() == System.Windows.Forms.DialogResult.OK && abrir.FileName.Length > 0)
                {
                    Area.LoadFile(abrir.FileName, RichTextBoxStreamType.PlainText);
                    pathEntrada = System.IO.Path.GetFullPath(abrir.FileName);
                    Pestana.Text = System.IO.Path.GetFileNameWithoutExtension(pathEntrada);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo abrir el archivo.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        public void GuardarComo()
        {
            guardar = new SaveFileDialog();
            guardar.Filter = "All Files (*.cbc)|*.cbc";

            if (guardar.ShowDialog() == System.Windows.Forms.DialogResult.OK && guardar.FileName.Length > 0)
            {
                RichTextBox ares = (RichTextBox)PestañaSeleccionda().GetNextControl(PestañaSeleccionda(), true);
                ares.SaveFile(guardar.FileName, RichTextBoxStreamType.PlainText);
                PathGuardado = System.IO.Path.GetFullPath(guardar.FileName);
            }
        }

        public void Guardar()
        {
            try
            {
                if (!pathEntrada.Equals("") && PathGuardado.Equals(""))
                {
                    RichTextBox ares = (RichTextBox)PestañaSeleccionda().GetNextControl(PestañaSeleccionda(), true);
                    ares.SaveFile(pathEntrada, RichTextBoxStreamType.PlainText);
                }
                else if (pathEntrada.Equals("") && PathGuardado.Equals(""))
                {
                    if (guardar.ShowDialog() == System.Windows.Forms.DialogResult.OK && guardar.FileName.Length > 0)
                    {
                        RichTextBox ares = (RichTextBox)PestañaSeleccionda().GetNextControl(PestañaSeleccionda(), true);
                        ares.SaveFile(guardar.FileName, RichTextBoxStreamType.PlainText);
                        PathGuardado = System.IO.Path.GetFullPath(guardar.FileName);
                    }
                }
                else if (pathEntrada.Equals("") && !PathGuardado.Equals(""))
                {
                    RichTextBox ares = (RichTextBox)PestañaSeleccionda().GetNextControl(PestañaSeleccionda(), true);
                    ares.SaveFile(PathGuardado, RichTextBoxStreamType.PlainText);
                }
                MessageBox.Show("Archivo Guardado.");
            }
            catch (Exception) { MessageBox.Show("Archivo NO Guardado."); }
        }
      
        #endregion
    }
}
