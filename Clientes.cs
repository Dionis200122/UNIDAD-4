using ActividadTres.TIENDAAD;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ActividadTres
{
    public partial class Clientes : Form
    {
        private readonly TIENDADOEntities _context; 
        private int ID;

        public Clientes()
        {
            InitializeComponent();
            _context = new TIENDADOEntities();
            this.Load += Clientes_Load;
            this.FormClosed += (s, e) => _context.Dispose();
            dgClientes.AutoGenerateColumns = true;
        }

        private void Clientes_Load(object sender, EventArgs e)
        {
            cargarDatos(GetID());
            limpiarInsertar();
            limpiarActualizar();
            txtEliminar.Clear();
        }

        private int GetID()
        {
            return ID;
        }

        private void cargarDatos(int iD)
        {
            try
            {
                var lista = _context.CLIENTEAD
                    .Select(c => new
                    {
                        iD = c.ClienteID,          
                        Nombre = c.NombreCompleto,   
                        Correo = c.CorreoElectronico,          
                        Telefono = c.Telefono,      
                        Direccion = c.Dirreccion    
                    })
                    .OrderBy(x => x.iD)
                    .ToList();

                dgClientes.DataSource = lista;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message);
            }
        }

        private static bool EmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void limpiarInsertar()
        {
            txtClienteID.Clear();
            txtNombreCompleto.Clear();
            txtCorreoEletronico.Clear();
            txtTelefono.Clear();
            txtDireccion.Clear();
        }

        private void limpiarActualizar()
        {
            textIDActualizar.Clear();
            txtNombreCompletoActualizar.Clear();
            txtCorreoelectronicoActualizar.Clear();
            txtTelefonoActualizar.Clear();
            txtDireccionActualizar.Clear();
        }

        private void btnCargar_Click(object sender, EventArgs e) => cargarDatos(GetID());

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreCompleto.Text) ||
                string.IsNullOrWhiteSpace(txtCorreoEletronico.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                MessageBox.Show("Complete todos los campos");
                return;
            }

            if (!EmailValido(txtCorreoEletronico.Text))
            {
                MessageBox.Show("Correo no válido");
                return;
            }

            var cliente = new CLIENTEAD
            {
                NombreCompleto = txtNombreCompleto.Text.Trim(),
                CorreoElectronico = txtCorreoEletronico.Text.Trim(),      
                Telefono = txtTelefono.Text.Trim(),            
                Dirreccion = txtDireccion.Text.Trim()        
            };

            try
            {
                _context.CLIENTEAD.Add(cliente);
                _context.SaveChanges();
                MessageBox.Show("Cliente agregado.");
                cargarDatos(GetID());
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
                MessageBox.Show("ID inválido");
                return;
            }

            var cliente = _context.CLIENTEAD.FirstOrDefault(c => c.ClienteID == id); 
            if (cliente == null)
            {
                MessageBox.Show("No encontrado");
                return;
            }

            if (MessageBox.Show("¿Eliminar?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _context.CLIENTEAD.Remove(cliente);
                _context.SaveChanges();
                MessageBox.Show("Eliminado");
                cargarDatos(GetID());
                txtEliminar.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar: " + ex.Message);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textIDActualizar.Text, out int id))
            {
                MessageBox.Show("ID inválido");
                return;
            }

            var cliente = _context.CLIENTEAD.FirstOrDefault(c => c.ClienteID == id); 
            if (cliente == null)
            {
                MessageBox.Show("No encontrado");
                return;
            }

            if (!EmailValido(txtCorreoelectronicoActualizar.Text))
            {
                MessageBox.Show("Correo inválido");
                return;
            }

            cliente.NombreCompleto = txtNombreCompletoActualizar.Text.Trim(); 
            cliente.CorreoElectronico = txtCorreoelectronicoActualizar.Text.Trim();    
            cliente.Telefono = txtTelefonoActualizar.Text.Trim();          
            cliente.Dirreccion = txtDireccionActualizar.Text.Trim();        

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Actualizado");
                cargarDatos(GetID());
                limpiarActualizar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar: " + ex.Message);
            }
        }

        private void dgClientes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgClientes.CurrentRow?.DataBoundItem == null) return;

            var row = dgClientes.CurrentRow;

            textIDActualizar.Text = row.Cells["ID"].Value?.ToString();
            txtNombreCompletoActualizar.Text = row.Cells["Nombre"].Value?.ToString();
            txtCorreoelectronicoActualizar.Text = row.Cells["Correo"].Value?.ToString();
            txtTelefonoActualizar.Text = row.Cells["Telefono"].Value?.ToString();
            txtDireccionActualizar.Text = row.Cells["Direccion"].Value?.ToString();

            txtEliminar.Text = row.Cells["ID"].Value?.ToString();
        }
    }
}
