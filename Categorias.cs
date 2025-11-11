using ActividadTres.TIENDAAD;
using System;
using System.Linq;
using System.Windows.Forms;

namespace ActividadTres
{
    public partial class Categorias : Form
    {
        private readonly TIENDADOEntities _context;

        public Categorias()
        {
            InitializeComponent();
            _context = new TIENDADOEntities();
            this.Load += Categorias_Load;
            this.FormClosed += (s, e) => _context.Dispose();
            dgCategorias.AutoGenerateColumns = true;
        }

        private void Categorias_Load(object sender, EventArgs e)
        {
            cargarDatos();
            limpiarInsertar();
            limpiarActualizar();
            txtEliminar.Clear();
        }

        private void cargarDatos()
        {
            try
            {
                var lista = _context.CATEGORIAD
                    .Select(c => new
                    {
                        ID = c.CATEGORIAID,
                        Nombre = c.NombreCategoria
                    })
                    .OrderBy(x => x.ID)
                    .ToList();

                dgCategorias.DataSource = lista;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message);
            }
        }

        private void limpiarInsertar()
        {
            txtCategoriaID.Clear();
            txtNombreCategoria.Clear();
        }

        private void limpiarActualizar()
        {
            txtCategoriaIDActualizar.Clear();
            txtNombreCategoriaActualizar.Clear();
        }

        private void btnCargar_Click(object sender, EventArgs e) => cargarDatos();

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreCategoria.Text))
            {
                MessageBox.Show("Ingrese el nombre de la categoría.");
                return;
            }

            var categoria = new CATEGORIAD
            {
                NombreCategoria = txtNombreCategoria.Text.Trim()
            };

            try
            {
                _context.CATEGORIAD.Add(categoria);
                _context.SaveChanges();
                MessageBox.Show("Categoría agregada correctamente.");
                cargarDatos();
                limpiarInsertar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar: " + ex.Message);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtEliminar.Text, out int id))
            {
                MessageBox.Show("Debe ingresar un ID válido.");
                return;
            }

            var categoria = _context.CATEGORIAD.FirstOrDefault(c => c.CATEGORIAID == id);
            if (categoria == null)
            {
                MessageBox.Show("Categoría no encontrada.");
                return;
            }

            if (MessageBox.Show("¿Eliminar esta categoría?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _context.CATEGORIAD.Remove(categoria);
                _context.SaveChanges();
                MessageBox.Show("Categoría eliminada.");
                cargarDatos();
                txtEliminar.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar: " + ex.Message);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtCategoriaIDActualizar.Text, out int id) ||
                string.IsNullOrWhiteSpace(txtNombreCategoriaActualizar.Text))
            {
                MessageBox.Show("Complete ID y Nombre.");
                return;
            }

            var categoria = _context.CATEGORIAD.FirstOrDefault(c => c.CATEGORIAID == id);
            if (categoria == null)
            {
                MessageBox.Show("Categoría no encontrada.");
                return;
            }

            categoria.NombreCategoria = txtNombreCategoriaActualizar.Text.Trim();

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Categoría actualizada correctamente.");
                cargarDatos();
                limpiarActualizar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar: " + ex.Message);
            }
        }

        private void dgCategorias_SelectionChanged(object sender, EventArgs e)
        {
            if (dgCategorias.CurrentRow?.DataBoundItem == null) return;

            var row = dgCategorias.CurrentRow;
            txtCategoriaIDActualizar.Text = row.Cells["ID"].Value?.ToString();
            txtNombreCategoriaActualizar.Text = row.Cells["Nombre"].Value?.ToString();
            txtEliminar.Text = row.Cells["ID"].Value?.ToString();
        }
    }
}
