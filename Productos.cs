using ActividadTres;
using ActividadTres.TIENDAAD;
using System;
using System.Data;
using System.Data.Entity; 
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ProductoEntity = ActividadTres.TIENDAAD.PRODUCTOSAD;
using CategoriaEntity = ActividadTres.TIENDAAD.CATEGORIAD;

namespace ActividadTres
{
    public partial class Productos : Form
    {
        private readonly TIENDADOEntities _context;

        public Productos()
        {
            InitializeComponent();
            _context = new TIENDADOEntities();

            if (cmbCategoria != null) cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            if (cmbCategoriaActualizar != null) cmbCategoriaActualizar.DropDownStyle = ComboBoxStyle.DropDownList;

            
            this.Load += Productos_Load;
            this.FormClosed += (s, e) => _context.Dispose();
        }

        private void Productos_Load(object sender, EventArgs e)
        {
            cargarCmbCategorias();
            cargarDatos();
            limpiarInsertar();
            limpiarActualizar();
            textID.Clear();
        }

        private void cargarDatos()
        {
            var lista = _context
                .Set<ProductoEntity>()
                .Include(p => p.CATEGORIAD)
                .Select(p => new
                {
                    p.ProductoID,
                    p.NombreProductoS,
                    p.Descripcion,
                    p.Precio,
                    p.Stock,
                    Categoria = p.CATEGORIAD.NombreCategoria
                })
                .OrderBy(x => x.ProductoID)
                .ToList();

            dgProductos.AutoGenerateColumns = true;
            dgProductos.DataSource = lista;
        }

        private void cargarCmbCategorias()
        {
            var categorias = _context
                .Set<CategoriaEntity>()
                .OrderBy(c => c.NombreCategoria)
                .Select(c => new { c.CATEGORIAID, c.NombreCategoria })
                .ToList();

            cmbCategoria.DataSource = categorias.ToList(); 
            cmbCategoria.DisplayMember = "NombreCategoria";
            cmbCategoria.ValueMember = "CategoriaID";
            cmbCategoria.SelectedIndex = -1;

            cmbCategoriaActualizar.DataSource = categorias.ToList();
            cmbCategoriaActualizar.DisplayMember = "NombreCategoria";
            cmbCategoriaActualizar.ValueMember = "CategoriaID";
            cmbCategoriaActualizar.SelectedIndex = -1;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!validarInsertar(out string nombre, out string desc, out int stock, out int categoriaId, out decimal precio))
                return;

            var producto = new ProductoEntity
            {
                NombreProductoS = nombre,
                Descripcion = desc,
                Stock = stock,
                CATEGORIAD = categoriaId,
                Precio = precio
            };

            try
            {
                _context.Set<ProductoEntity>().Add(producto);
                _context.SaveChanges();
                MessageBox.Show("Producto guardado correctamente.");
                cargarDatos();
                limpiarInsertar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el producto: " + ex.Message);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textID.Text, out int productoID))
            {
                MessageBox.Show("Debe ingresar un ID válido.");
                return;
            }

            var producto = _context.Set<ProductoEntity>().FirstOrDefault(p => p.ProductoID == productoID);
            if (producto == null)
            {
                MessageBox.Show("Producto no encontrado.");
                return;
            }

            if (MessageBox.Show("¿Eliminar el producto?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _context.Set<ProductoEntity>().Remove(producto);
                _context.SaveChanges();
                MessageBox.Show("Producto eliminado correctamente.");
                cargarDatos();
                textID.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el producto: " + ex.Message);
            }
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            if (!validarActualizar(out int productoID, out string nombre, out string desc, out int stock, out int categoriaId, out decimal precio))
                return;

            var producto = _context.Set<ProductoEntity>().FirstOrDefault(p => p.ProductoID == productoID);
            if (producto == null)
            {
                MessageBox.Show("Producto no encontrado.");
                return;
            }

            producto.NombreProductoS = nombre;
            producto.Descripcion = desc;
            producto.Stock = stock;
            producto.CATEGORIAD = categoriaId;
            producto.Precio = precio;

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Producto actualizado correctamente.");
                cargarDatos();
                limpiarActualizar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el producto: " + ex.Message);
            }
        }

        private void btnCargar_Click(object sender, EventArgs e) => cargarDatos();

        private bool validarInsertar(out string nombre, out string desc, out int stock, out int categoriaId, out decimal precio)
        {
            nombre = textNombreCompleto.Text.Trim();
            desc = textDescripcion.Text.Trim();
            stock = 0; categoriaId = 0; precio = 0;

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(desc))
            {
                MessageBox.Show("Ingrese nombre y descripción.");
                return false;
            }
            if (!int.TryParse(textStock.Text, out stock))
            {
                MessageBox.Show("Stock inválido.");
                return false;
            }
            if (cmbCategoria.SelectedValue == null || !int.TryParse(cmbCategoria.SelectedValue.ToString(), out categoriaId))
            {
                MessageBox.Show("Seleccione una categoría.");
                return false;
            }
            if (!decimal.TryParse(textPrecio.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out precio))
            {
                if (!decimal.TryParse(textPrecio.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out precio))
                {
                    MessageBox.Show("Precio inválido.");
                    return false;
                }
            }
            return true;
        }

        private bool validarActualizar(out int productoID, out string nombre, out string desc, out int stock, out int categoriaId, out decimal precio)
        {
            productoID = 0; nombre = ""; desc = ""; stock = 0; categoriaId = 0; precio = 0;

            if (!int.TryParse(textIDActualizar.Text, out productoID))
            {
                MessageBox.Show("ID de producto inválido.");
                return false;
            }

            nombre = textNombreCompletoActualizar.Text.Trim();
            desc = textDescripsionActualizar.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(desc))
            {
                MessageBox.Show("Ingrese nombre y descripción.");
                return false;
            }

            if (!int.TryParse(textStockActualizar.Text, out stock))
            {
                MessageBox.Show("Stock inválido.");
                return false;
            }

            if (cmbCategoriaActualizar.SelectedValue == null || !int.TryParse(cmbCategoriaActualizar.SelectedValue.ToString(), out categoriaId))
            {
                MessageBox.Show("Seleccione una categoría.");
                return false;
            }

            if (!decimal.TryParse(textPrecioActualizar.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out precio))
            {
                if (!decimal.TryParse(textPrecioActualizar.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out precio))
                {
                    MessageBox.Show("Precio inválido.");
                    return false;
                }
            }
            return true;
        }

        private void limpiarInsertar()
        {
            textNombreCompleto.Clear();
            textDescripcion.Clear();
            textStock.Clear();
            textPrecio.Clear();
            cmbCategoria.SelectedIndex = -1;
        }

        private void limpiarActualizar()
        {
            textIDActualizar.Clear();
            textNombreCompletoActualizar.Clear();
            textDescripsionActualizar.Clear();
            textStockActualizar.Clear();
            textPrecioActualizar.Clear();
            cmbCategoriaActualizar.SelectedIndex = -1;
        }

        private void dgProductos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgProductos.CurrentRow?.DataBoundItem == null) return;

            var row = dgProductos.CurrentRow;
            textIDActualizar.Text = row.Cells["ProductoID"].Value?.ToString();
            textNombreCompletoActualizar.Text = row.Cells["NombreProducto"].Value?.ToString();
            textDescripsionActualizar.Text = row.Cells["Descripcion"].Value?.ToString();
            textStockActualizar.Text = row.Cells["Stock"].Value?.ToString();
            textPrecioActualizar.Text = row.Cells["Precio"].Value?.ToString();

            var nombreCat = row.Cells["Categoria"].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(nombreCat))
            {
                for (int i = 0; i < cmbCategoriaActualizar.Items.Count; i++)
                {
                    var item = cmbCategoriaActualizar.Items[i];
                    var propNombre = item.GetType().GetProperty("NombreCategoria");
                    if (propNombre != null && (propNombre.GetValue(item)?.ToString() ?? "") == nombreCat)
                    {
                        cmbCategoriaActualizar.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
    }
}
