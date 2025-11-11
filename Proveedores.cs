using ActividadTres.TIENDAAD;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ProveedorEntity = ActividadTres.TIENDAAD.PROVEEDORESAD;

namespace ActividadTres
{
    public partial class Proveedores : Form
    {
        private readonly TIENDADOEntities _context;

        public Proveedores()
        {
            InitializeComponent();
            _context = new TIENDADOEntities();
            this.Load += Proveedores_Load;
            this.FormClosed += (s, e) => _context.Dispose();
            dgProveedores.AutoGenerateColumns = true;
        }

        private void Proveedores_Load(object sender, EventArgs e)
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
                var lista = _context.PROVEEDORESAD
                    .Select(p => new
                    {
                        ID = p.ProveedorID,
                        Nombre = p.NombreProveedor,
                        Telefono = p.Telefono,
                        Correo = p.CorreoElectronico
                    })
                    .OrderBy(x => x.ID)
                    .ToList();

                dgProveedores.DataSource = lista;
            }
            catch (Exception ex)
            {
                MostrarError(ex, "cargar proveedores");
            }
        }

        private static bool EmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void limpiarInsertar()
        {
            txtProveedorID.Clear();
            txtNombreProveedor.Clear();
            txtTelefono.Clear();
            txtCorreoElectronico.Clear();
        }

        private void limpiarActualizar()
        {
            textProveedorIDActualizar.Clear();
            txtProveedorActualizar.Clear();
            txtTelefonoActualizar.Clear();
            txtCorreoElectronicoActualizar.Clear();
        }

        private void MostrarError(Exception ex, string accion)
        {
            string msg = $"Error al {accion}:\n{ex.Message}";
            var i = ex.InnerException;
            while (i != null)
            {
                msg += "\n→ " + i.Message;
                i = i.InnerException;
            }
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnCargar_Click(object sender, EventArgs e) => cargarDatos();

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreProveedor.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtCorreoElectronico.Text))
            {
                MessageBox.Show("Complete Nombre, Teléfono y Correo.");
                return;
            }

            if (!EmailValido(txtCorreoElectronico.Text))
            {
                MessageBox.Show("Correo no válido.");
                return;
            }

            var proveedor = new ProveedorEntity
            {
                NombreProveedor = txtNombreProveedor.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                CorreoElectronico = txtCorreoElectronico.Text.Trim()
            };

            try
            {
                _context.PROVEEDORESAD.Add(proveedor);
                _context.SaveChanges();
                MessageBox.Show("Proveedor agregado.");
                cargarDatos();
                limpiarInsertar();
            }
            catch (Exception ex)
            {
                MostrarError(ex, "agregar proveedor");
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtEliminar.Text, out int id))
            {
                MessageBox.Show("ID inválido.");
                return;
            }

            var proveedor = _context.PROVEEDORESAD.FirstOrDefault(p => p.ProveedorID == id);
            if (proveedor == null)
            {
                MessageBox.Show("Proveedor no encontrado.");
                return;
            }

            if (MessageBox.Show("¿Eliminar este proveedor?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _context.PROVEEDORESAD.Remove(proveedor);
                _context.SaveChanges();
                MessageBox.Show("Proveedor eliminado.");
                cargarDatos();
                txtEliminar.Clear();
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("DELETE statement conflicted"))
                {
                    MessageBox.Show("No se puede eliminar: hay registros relacionados.",
                        "Restricción de clave foránea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                MostrarError(ex, "eliminar proveedor");
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textProveedorIDActualizar.Text, out int id) ||
                string.IsNullOrWhiteSpace(txtProveedorActualizar.Text) ||
                string.IsNullOrWhiteSpace(txtTelefonoActualizar.Text) ||
                string.IsNullOrWhiteSpace(txtCorreoElectronicoActualizar.Text))
            {
                MessageBox.Show("Complete todos los campos correctamente.");
                return;
            }

            if (!EmailValido(txtCorreoElectronicoActualizar.Text))
            {
                MessageBox.Show("Correo no válido.");
                return;
            }

            var proveedor = _context.PROVEEDORESAD.FirstOrDefault(p => p.ProveedorID == id);
            if (proveedor == null)
            {
                MessageBox.Show("Proveedor no encontrado.");
                return;
            }

            proveedor.NombreProveedor = txtProveedorActualizar.Text.Trim();
            proveedor.Telefono = txtTelefonoActualizar.Text.Trim();
            proveedor.CorreoElectronico = txtCorreoElectronicoActualizar.Text.Trim();

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Proveedor actualizado.");
                cargarDatos();
                limpiarActualizar();
            }
            catch (Exception ex)
            {
                MostrarError(ex, "actualizar proveedor");
            }
        }

        private void dgProveedores_SelectionChanged(object sender, EventArgs e)
        {
            if (dgProveedores.CurrentRow?.DataBoundItem == null) return;

            var row = dgProveedores.CurrentRow;
            textProveedorIDActualizar.Text = row.Cells["ID"].Value?.ToString();
            txtProveedorActualizar.Text = row.Cells["Nombre"].Value?.ToString();
            txtTelefonoActualizar.Text = row.Cells["Telefono"].Value?.ToString();
            txtCorreoElectronicoActualizar.Text = row.Cells["Correo"].Value?.ToString();

            txtEliminar.Text = row.Cells["ID"].Value?.ToString();
        }
    }
}
