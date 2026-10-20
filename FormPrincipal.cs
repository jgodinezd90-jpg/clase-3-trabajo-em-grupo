namespace InventarioTienda;


/// Ventana principal del sistema de inventario.
/// Aquí se usan las TRES estructuras de datos:
///   1) VECTOR (arreglo)  -> existenciasPorCategoria  (total de unidades por categoría)
///   2) ARRAYLIST (List)  -> catalogo                 (productos de la tienda)
///   3) LISTA ENLAZADA    -> movimientos              (historial de entradas y salidas)

public class FormPrincipal : Form
{
    // =====================================================================
    //  ESTRUCTURAS DE DATOS
    // =====================================================================

    // Nombres de las categorías (arreglo fijo de texto)
    private readonly string[] categorias = { "Dama", "Caballero", "Niño", "Niña", "Tallas Plus", "Calzado", "Otros" };

    // 1) VECTOR: la posición i guarda el total de unidades de la categoría categorias[i].
    //    Ejemplo: existenciasPorCategoria[0] = unidades de "Alimentos".
    private readonly int[] existenciasPorCategoria;

    // 2) ARRAYLIST: catálogo de productos. Crece y decrece dinámicamente.
    private readonly List<Producto> catalogo = new List<Producto>();

    // 3) LISTA ENLAZADA propia: historial de movimientos (ver ListaMovimientos.cs)
    private readonly ListaMovimientos movimientos = new ListaMovimientos();

    // Contador para darle un Id único a cada movimiento
    private int siguienteIdMovimiento = 1;

    // Bandera para evitar eventos mientras se recarga la tabla
    private bool cargando = false;

    //  COLORES (para que se vea bonito)

    private readonly Color colorOscuro = Color.FromArgb(31, 42, 68);
    private readonly Color colorFondo = Color.FromArgb(244, 246, 250);
    private readonly Color colorVerde = Color.FromArgb(46, 125, 50);
    private readonly Color colorAzul = Color.FromArgb(25, 118, 210);
    private readonly Color colorRojo = Color.FromArgb(211, 47, 47);
    private readonly Color colorNaranja = Color.FromArgb(245, 124, 0);
    private readonly Color colorGris = Color.FromArgb(96, 125, 139);

    //  CONTROLES DE LA INTERFAZ

    private TextBox txtCodigo, txtNombre, txtBuscar;
    private ComboBox cmbCategoria, cmbOrden;
    private NumericUpDown numPrecio, numExistencia, numCantidad;
    private DataGridView gridProductos, gridMovimientos;
    private ListView lstCategorias;
    private Label lblResumen;


    //  CONSTRUCTOR
    public FormPrincipal()
    {
        // Inicialización del vector: se crea con un espacio por cada categoría
        // (todas las posiciones empiezan en 0)
        existenciasPorCategoria = new int[categorias.Length];

        ConstruirInterfaz();
        ActualizarVista();
    }


    //  CONSTRUCCIÓN DE LA INTERFAZ  
    private void ConstruirInterfaz()
    {
        Text = "Inventario de Tienda";
        Size = new Size(1250, 740);
        MinimumSize = new Size(1100, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = colorFondo;
        Font = new Font("Segoe UI", 9.5f);

        // ---------- Panel central (tabla de productos) ----------
        // Nota: en WinForms el control con Dock=Fill debe agregarse primero.
        Panel pnlCentro = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        Controls.Add(pnlCentro);

        // ---------- Panel derecho (categorías y movimientos) ----------
        Panel pnlDerecha = new Panel { Dock = DockStyle.Right, Width = 440, Padding = new Padding(10) };
        Controls.Add(pnlDerecha);

        // ---------- Panel del formulario de producto ----------
        Panel pnlForm = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = Color.White };
        Controls.Add(pnlForm);

        // ---------- Encabezado ----------
        Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = colorOscuro };
        Controls.Add(pnlHeader);

        // ===== ENCABEZADO =====
        pnlHeader.Controls.Add(new Label
        {
            Text = "Inventario de Tienda",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Left = 20,
            Top = 12
        });
        lblResumen = new Label
        {
            Dock = DockStyle.Right,
            Width = 420,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 20, 0)
        };
        pnlHeader.Controls.Add(lblResumen);

        // ===== FORMULARIO DE PRODUCTO =====
        pnlForm.Controls.Add(Etiqueta("Código", 20, 12));
        txtCodigo = new TextBox { Left = 20, Top = 35, Width = 110, MaxLength = 15 };
        pnlForm.Controls.Add(txtCodigo);

        pnlForm.Controls.Add(Etiqueta("Nombre", 145, 12));
        txtNombre = new TextBox { Left = 145, Top = 35, Width = 200, MaxLength = 60 };
        pnlForm.Controls.Add(txtNombre);

        pnlForm.Controls.Add(Etiqueta("Categoría", 360, 12));
        cmbCategoria = new ComboBox { Left = 360, Top = 35, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbCategoria.Items.AddRange(categorias);   // Llena el combo con el vector de categorías
        cmbCategoria.SelectedIndex = 0;
        pnlForm.Controls.Add(cmbCategoria);

        pnlForm.Controls.Add(Etiqueta("Precio (₡)", 515, 12));
        numPrecio = new NumericUpDown
        {
            Left = 515,
            Top = 35,
            Width = 110,
            DecimalPlaces = 2,
            Maximum = 100000000,
            Increment = 50,
            ThousandsSeparator = true
        };
        pnlForm.Controls.Add(numPrecio);

        pnlForm.Controls.Add(Etiqueta("Existencia", 640, 12));
        numExistencia = new NumericUpDown { Left = 640, Top = 35, Width = 90, Maximum = 100000 };
        pnlForm.Controls.Add(numExistencia);

        pnlForm.Controls.Add(CrearBoton("Registrar", colorVerde, 20, 80, 120, BtnRegistrar_Click));
        pnlForm.Controls.Add(CrearBoton("Modificar", colorAzul, 150, 80, 120, BtnModificar_Click));
        pnlForm.Controls.Add(CrearBoton("Eliminar", colorRojo, 280, 80, 120, BtnEliminar_Click));
        pnlForm.Controls.Add(CrearBoton("Limpiar", colorGris, 410, 80, 110, (s, e) => LimpiarFormulario()));

        // ===== PANEL CENTRAL: búsqueda, tabla y entrada/salida =====
        gridProductos = CrearGridProductos();
        pnlCentro.Controls.Add(gridProductos);

        Panel pnlStock = new Panel { Dock = DockStyle.Bottom, Height = 55 };
        pnlCentro.Controls.Add(pnlStock);

        Panel pnlBusqueda = new Panel { Dock = DockStyle.Top, Height = 50 };
        pnlCentro.Controls.Add(pnlBusqueda);

        // Barra de búsqueda y ordenamiento
        pnlBusqueda.Controls.Add(Etiqueta("Buscar:", 0, 16));
        txtBuscar = new TextBox { Left = 60, Top = 12, Width = 220 };
        txtBuscar.TextChanged += (s, e) => MostrarProductos();   // Filtra mientras se escribe
        pnlBusqueda.Controls.Add(txtBuscar);

        pnlBusqueda.Controls.Add(Etiqueta("Ordenar:", 300, 16));
        cmbOrden = new ComboBox { Left = 365, Top = 12, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbOrden.Items.AddRange(new object[]
        {
            "Nombre (A-Z)", "Nombre (Z-A)", "Precio (menor a mayor)", "Precio (mayor a menor)"
        });
        cmbOrden.SelectedIndex = 0;
        pnlBusqueda.Controls.Add(cmbOrden);
        pnlBusqueda.Controls.Add(CrearBoton("Ordenar", colorAzul, 575, 8, 100, BtnOrdenar_Click));

        // Barra de entrada / salida de mercadería
        pnlStock.Controls.Add(Etiqueta("Cantidad:", 0, 20));
        numCantidad = new NumericUpDown { Left = 70, Top = 16, Width = 80, Minimum = 1, Maximum = 100000, Value = 1 };
        pnlStock.Controls.Add(numCantidad);
        pnlStock.Controls.Add(CrearBoton("Entrada (+)", colorVerde, 165, 12, 130, (s, e) => MoverStock(true)));
        pnlStock.Controls.Add(CrearBoton("Salida (-)", colorRojo, 305, 12, 130, (s, e) => MoverStock(false)));


        // ===== PANEL DERECHO =====
        // Orden de agregado: lo que va en Fill primero, luego el resto.
        gridMovimientos = CrearGridMovimientos();
        pnlDerecha.Controls.Add(gridMovimientos);

        Panel pnlBotonesMov = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        pnlBotonesMov.Controls.Add(CrearBoton("Ver todos", colorGris, 0, 10, 125, BtnVerTodosMov_Click));
        pnlBotonesMov.Controls.Add(CrearBoton("Buscar producto", colorAzul, 133, 10, 150, BtnBuscarMov_Click));
        pnlBotonesMov.Controls.Add(CrearBoton("Eliminar", colorRojo, 291, 10, 125, BtnEliminarMov_Click));
        pnlDerecha.Controls.Add(pnlBotonesMov);

        pnlDerecha.Controls.Add(Titulo("Movimientos (lista enlazada)"));

        lstCategorias = new ListView
        {
            Dock = DockStyle.Top,
            Height = 190,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            BorderStyle = BorderStyle.None
        };
        lstCategorias.Columns.Add("Categoría", 250);
        lstCategorias.Columns.Add("Existencias", 150, HorizontalAlignment.Right);
        pnlDerecha.Controls.Add(lstCategorias);

        pnlDerecha.Controls.Add(Titulo("Existencias por categoría (vector)"));
    }

    //  Ayudas para crear controles con el mismo estilo
    private Button CrearBoton(string texto, Color color, int x, int y, int ancho, EventHandler accion)
    {
        Button b = new Button
        {
            Text = texto,
            Left = x,
            Top = y,
            Width = ancho,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        b.Click += accion;
        return b;
    }

    private Label Etiqueta(string texto, int x, int y)
    {
        return new Label { Text = texto, Left = x, Top = y, AutoSize = true, ForeColor = Color.FromArgb(70, 80, 100) };
    }

    private Label Titulo(string texto)
    {
        return new Label
        {
            Text = texto,
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = colorOscuro,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    // Estilo común para las dos tablas
    private void EstilizarGrid(DataGridView g)
    {
        g.Dock = DockStyle.Fill;
        g.AutoGenerateColumns = false;
        g.ReadOnly = true;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.MultiSelect = false;
        g.BackgroundColor = Color.White;
        g.BorderStyle = BorderStyle.None;
        g.RowHeadersVisible = false;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersDefaultCellStyle.BackColor = colorOscuro;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        g.ColumnHeadersHeight = 34;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.RowTemplate.Height = 28;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 252);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(187, 222, 251);
        g.DefaultCellStyle.SelectionForeColor = Color.Black;
    }

    private DataGridView CrearGridProductos()
    {
        DataGridView g = new DataGridView();
        EstilizarGrid(g);
        g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código", DataPropertyName = "Codigo", FillWeight = 15 });
        g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", FillWeight = 35 });
        g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoría", DataPropertyName = "Categoria", FillWeight = 20 });
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Precio (₡)",
            DataPropertyName = "Precio",
            FillWeight = 18,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Existencia",
            DataPropertyName = "Existencia",
            FillWeight = 14,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
        });

        // Al seleccionar una fila, sus datos pasan al formulario para poder modificarlos
        g.SelectionChanged += GridProductos_SelectionChanged;
        return g;
    }

    private DataGridView CrearGridMovimientos()
    {
        DataGridView g = new DataGridView();
        EstilizarGrid(g);
        g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "Id", FillWeight = 10 });
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Fecha",
            DataPropertyName = "Fecha",
            FillWeight = 25,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM HH:mm" }
        });
        g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "Producto", FillWeight = 40 });
        g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tipo", DataPropertyName = "Tipo", FillWeight = 22 });
        g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cant.", DataPropertyName = "Cantidad", FillWeight = 12 });
        return g;
    }

    // =====================================================================
    //  OPERACIONES SOBRE EL ARRAYLIST (catálogo de productos)
    // =====================================================================

    // REGISTRAR: agrega un producto nuevo al ArrayList
    private void BtnRegistrar_Click(object sender, EventArgs e)
    {
        if (!ValidarFormulario(out string error))
        {
            Error(error);
            return;
        }

        string codigo = txtCodigo.Text.Trim().ToUpper();

        // No se permiten códigos repetidos
        if (BuscarIndicePorCodigo(codigo) >= 0)
        {
            Error("Ya existe un producto con el código " + codigo + ".");
            return;
        }

        Producto nuevo = new Producto
        {
            Codigo = codigo,
            Nombre = txtNombre.Text.Trim(),
            Categoria = cmbCategoria.SelectedItem.ToString(),
            Precio = numPrecio.Value,
            Existencia = (int)numExistencia.Value
        };

        catalogo.Add(nuevo);                                              // ArrayList: agregar
        AgregarMovimiento(nuevo, "Registro", nuevo.Existencia);           // Lista enlazada: insertar
        LimpiarFormulario();
        ActualizarVista();
        SeleccionarPorCodigo(codigo);
    }

    // MODIFICAR: cambia los datos de un producto existente
    private void BtnModificar_Click(object sender, EventArgs e)
    {
        if (catalogo.Count == 0)
        {
            Error("No hay productos registrados para modificar.");
            return;
        }
        if (!ValidarFormulario(out string error))
        {
            Error(error);
            return;
        }

        string codigo = txtCodigo.Text.Trim().ToUpper();
        int indice = BuscarIndicePorCodigo(codigo);
        if (indice < 0)
        {
            Error("No existe un producto con ese código.\nSeleccione uno de la tabla para modificarlo.");
            return;
        }

        Producto p = catalogo[indice];              // ArrayList: consultar por índice
        int existenciaAnterior = p.Existencia;

        p.Nombre = txtNombre.Text.Trim();           // ArrayList: modificar
        p.Categoria = cmbCategoria.SelectedItem.ToString();
        p.Precio = numPrecio.Value;
        p.Existencia = (int)numExistencia.Value;

        // Si cambió la existencia, se deja constancia en el historial
        int diferencia = p.Existencia - existenciaAnterior;
        if (diferencia != 0)
            AgregarMovimiento(p, "Ajuste", diferencia);

        ActualizarVista();
        SeleccionarPorCodigo(codigo);
        Info("Producto modificado correctamente.");
    }

    // ELIMINAR: quita el producto seleccionado del ArrayList
    private void BtnEliminar_Click(object sender, EventArgs e)
    {
        if (catalogo.Count == 0)
        {
            Error("No hay productos registrados para eliminar.");
            return;
        }

        Producto p = ObtenerSeleccionado();
        if (p == null)
        {
            Error("Seleccione en la tabla el producto que desea eliminar.");
            return;
        }

        if (MessageBox.Show("¿Eliminar el producto \"" + p.Nombre + "\"?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        int indice = BuscarIndicePorCodigo(p.Codigo);
        catalogo.RemoveAt(indice);                      // ArrayList: eliminar
        AgregarMovimiento(p, "Eliminación", p.Existencia);

        LimpiarFormulario();
        ActualizarVista();
    }

    // ORDENAR: ordena el ArrayList según el criterio elegido
    private void BtnOrdenar_Click(object sender, EventArgs e)
    {
        if (catalogo.Count == 0)
        {
            Error("No hay productos para ordenar.");
            return;
        }

        switch (cmbOrden.SelectedIndex)
        {
            case 0: // Nombre A-Z
                catalogo.Sort((a, b) => string.Compare(a.Nombre, b.Nombre, StringComparison.OrdinalIgnoreCase));
                break;
            case 1: // Nombre Z-A
                catalogo.Sort((a, b) => string.Compare(b.Nombre, a.Nombre, StringComparison.OrdinalIgnoreCase));
                break;
            case 2: // Precio menor a mayor
                catalogo.Sort((a, b) => a.Precio.CompareTo(b.Precio));
                break;
            case 3: // Precio mayor a menor
                catalogo.Sort((a, b) => b.Precio.CompareTo(a.Precio));
                break;
        }
        MostrarProductos();
    }

    // ENTRADA / SALIDA: actualiza las existencias del producto seleccionado
    private void MoverStock(bool esEntrada)
    {
        if (catalogo.Count == 0)
        {
            Error("No hay productos registrados.");
            return;
        }

        Producto p = ObtenerSeleccionado();
        if (p == null)
        {
            Error("Seleccione un producto de la tabla.");
            return;
        }

        int cantidad = (int)numCantidad.Value;

        // Validación: no se puede sacar más de lo que hay
        if (!esEntrada && cantidad > p.Existencia)
        {
            Error("No hay suficientes existencias.\nDisponibles: " + p.Existencia);
            return;
        }

        p.Existencia += esEntrada ? cantidad : -cantidad;
        AgregarMovimiento(p, esEntrada ? "Entrada" : "Salida", cantidad);

        ActualizarVista();
        SeleccionarPorCodigo(p.Codigo);
    }

    // Carga productos de ejemplo para la demostración
    //private void BtnEjemplo_Click(object sender, EventArgs e)
    //{
    //object[,] datos =
    //{
    //    { "A001", "Arroz 1kg", "Alimentos", 1250m, 40 },
    //    { "A002", "Frijoles 500g", "Alimentos", 980m, 25 },
    //    { "B001", "Agua 600ml", "Bebidas", 700m, 60 },
    //    { "B002", "Jugo de naranja 1L", "Bebidas", 1800m, 3 },
    //    { "L001", "Detergente 1kg", "Limpieza", 3200m, 15 },
    //    { "L002", "Jabón de baño", "Limpieza", 850m, 4 },
    //    { "E001", "Cable USB-C", "Electrónica", 3500m, 12 },
    //    { "O001", "Cuaderno 100 hojas", "Otros", 1500m, 30 }
    //};

    //    int agregados = 0;
    //    for (int i = 0; i < datos.GetLength(0); i++)
    //    {
    //        string codigo = (string)datos[i, 0];
    //        if (BuscarIndicePorCodigo(codigo) >= 0) continue;   // Evita duplicados

    //        Producto p = new Producto
    //        {
    //            Codigo = codigo,
    //            Nombre = (string)datos[i, 1],
    //            Categoria = (string)datos[i, 2],
    //            Precio = (decimal)datos[i, 3],
    //            Existencia = (int)datos[i, 4]
    //        };
    //        catalogo.Add(p);
    //        AgregarMovimiento(p, "Registro", p.Existencia);
    //        agregados++;
    //    }

    //    ActualizarVista();
    //    Info(agregados + " producto(s) de ejemplo cargados.");
    //}

    // =====================================================================
    //  OPERACIONES SOBRE LA LISTA ENLAZADA (movimientos)
    // =====================================================================

    // Crea un movimiento y lo inserta en la lista enlazada
    private void AgregarMovimiento(Producto p, string tipo, int cantidad)
    {
        Movimiento m = new Movimiento
        {
            Id = siguienteIdMovimiento++,
            Fecha = DateTime.Now,
            CodigoProducto = p.Codigo,
            NombreProducto = p.Nombre,
            Tipo = tipo,
            Cantidad = cantidad
        };
        movimientos.InsertarAlInicio(m);    // Inserción en la lista enlazada
    }

    // RECORRIDO: muestra todos los movimientos
    private void BtnVerTodosMov_Click(object sender, EventArgs e)
    {
        if (movimientos.EstaVacia)
        {
            Info("Todavía no hay movimientos registrados.");
            return;
        }
        MostrarMovimientos(movimientos.Recorrer());
    }

    // BÚSQUEDA: muestra solo los movimientos del producto seleccionado
    private void BtnBuscarMov_Click(object sender, EventArgs e)
    {
        if (movimientos.EstaVacia)
        {
            Info("Todavía no hay movimientos registrados.");
            return;
        }

        Producto p = ObtenerSeleccionado();
        string codigo = p != null ? p.Codigo : txtCodigo.Text.Trim();
        if (string.IsNullOrWhiteSpace(codigo))
        {
            Error("Seleccione un producto o escriba su código en el formulario.");
            return;
        }

        List<Movimiento> encontrados = movimientos.BuscarPorCodigo(codigo);
        if (encontrados.Count == 0)
        {
            Info("No hay movimientos para el código " + codigo.ToUpper() + ".");
            return;
        }
        MostrarMovimientos(encontrados);
    }

    // ELIMINACIÓN: borra el movimiento seleccionado de la lista enlazada
    private void BtnEliminarMov_Click(object sender, EventArgs e)
    {
        if (movimientos.EstaVacia)
        {
            Error("No hay movimientos para eliminar.");
            return;
        }

        if (gridMovimientos.SelectedRows.Count == 0 ||
            !(gridMovimientos.SelectedRows[0].DataBoundItem is Movimiento m))
        {
            Error("Seleccione un movimiento de la tabla.");
            return;
        }

        if (MessageBox.Show("¿Eliminar el movimiento #" + m.Id + " del historial?\n" +
                "(No modifica las existencias del producto.)", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        movimientos.EliminarPorId(m.Id);
        ActualizarVista();
    }

    // =====================================================================
    //  OPERACIONES SOBRE EL VECTOR (existencias por categoría)
    // =====================================================================

    // Recalcula el vector: pone todo en 0 y luego suma las existencias de cada producto
    private void RecalcularExistencias()
    {
        // Recorrido del vector para reiniciarlo
        for (int i = 0; i < existenciasPorCategoria.Length; i++)
            existenciasPorCategoria[i] = 0;

        // Recorrido del ArrayList; el índice de la categoría decide en qué posición del vector se suma
        foreach (Producto p in catalogo)
        {
            int posicion = Array.IndexOf(categorias, p.Categoria);
            if (posicion >= 0)
                existenciasPorCategoria[posicion] += p.Existencia;   // Acceso por índice
        }
    }

    // =====================================================================
    //  MOSTRAR DATOS EN PANTALLA
    // =====================================================================

    // Refresca todo lo que se ve en la ventana
    private void ActualizarVista()
    {
        RecalcularExistencias();
        MostrarProductos();
        MostrarCategorias();
        MostrarMovimientos(movimientos.Recorrer());

        lblResumen.Text = "Productos: " + catalogo.Count + "   |   Movimientos: " + movimientos.Cantidad;
    }

    // Muestra el catálogo (aplicando el filtro de búsqueda si hay texto)
    private void MostrarProductos()
    {
        cargando = true;

        string filtro = txtBuscar.Text.Trim().ToLower();
        List<Producto> visibles = new List<Producto>();

        // BÚSQUEDA en el ArrayList: por nombre o código
        foreach (Producto p in catalogo)
        {
            if (filtro == "" || p.Nombre.ToLower().Contains(filtro) || p.Codigo.ToLower().Contains(filtro))
                visibles.Add(p);
        }

        gridProductos.DataSource = null;
        gridProductos.DataSource = visibles;

        // Resalta los productos con pocas existencias
        foreach (DataGridViewRow fila in gridProductos.Rows)
        {
            if (fila.DataBoundItem is Producto p && p.Existencia <= 5)
                fila.DefaultCellStyle.BackColor = Color.MistyRose;
        }

        gridProductos.ClearSelection();
        cargando = false;
    }

    // Muestra el vector en la lista de categorías
    private void MostrarCategorias()
    {
        lstCategorias.Items.Clear();
        int total = 0;

        // RECORRIDO del vector por índice
        for (int i = 0; i < categorias.Length; i++)
        {
            ListViewItem item = new ListViewItem(categorias[i]);
            item.SubItems.Add(existenciasPorCategoria[i].ToString());
            lstCategorias.Items.Add(item);
            total += existenciasPorCategoria[i];
        }

        ListViewItem fila = new ListViewItem("TOTAL");
        fila.SubItems.Add(total.ToString());
        fila.Font = new Font(lstCategorias.Font, FontStyle.Bold);
        lstCategorias.Items.Add(fila);
    }

    // Muestra una lista de movimientos en la tabla de la derecha
    private void MostrarMovimientos(List<Movimiento> lista)
    {
        gridMovimientos.DataSource = null;
        gridMovimientos.DataSource = lista;
        gridMovimientos.ClearSelection();
    }

    // =====================================================================
    //  FUNCIONES DE APOYO
    // =====================================================================

    // Busca un producto por código en el ArrayList; devuelve su índice o -1 si no existe
    private int BuscarIndicePorCodigo(string codigo)
    {
        for (int i = 0; i < catalogo.Count; i++)
        {
            if (string.Equals(catalogo[i].Codigo, codigo, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    // Devuelve el producto seleccionado en la tabla (o null si no hay ninguno)
    private Producto ObtenerSeleccionado()
    {
        if (gridProductos.SelectedRows.Count == 0) return null;
        return gridProductos.SelectedRows[0].DataBoundItem as Producto;
    }

    // Vuelve a seleccionar en la tabla el producto con ese código
    private void SeleccionarPorCodigo(string codigo)
    {
        foreach (DataGridViewRow fila in gridProductos.Rows)
        {
            if (fila.DataBoundItem is Producto p &&
                string.Equals(p.Codigo, codigo, StringComparison.OrdinalIgnoreCase))
            {
                fila.Selected = true;
                gridProductos.CurrentCell = fila.Cells[0];
                return;
            }
        }
    }

    // Al seleccionar una fila, copia sus datos al formulario
    private void GridProductos_SelectionChanged(object sender, EventArgs e)
    {
        if (cargando) return;
        Producto p = ObtenerSeleccionado();
        if (p == null) return;

        txtCodigo.Text = p.Codigo;
        txtNombre.Text = p.Nombre;
        cmbCategoria.SelectedItem = p.Categoria;
        numPrecio.Value = p.Precio;
        numExistencia.Value = p.Existencia;
    }

    // Valida los campos del formulario (obligatorios y datos inválidos)
    private bool ValidarFormulario(out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(txtCodigo.Text))
        {
            error = "El código es obligatorio.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            error = "El nombre es obligatorio.";
            return false;
        }
        if (cmbCategoria.SelectedIndex < 0)
        {
            error = "Seleccione una categoría.";
            return false;
        }
        if (numPrecio.Value <= 0)
        {
            error = "El precio debe ser mayor que cero.";
            return false;
        }
        return true;
    }

    private void LimpiarFormulario()
    {
        txtCodigo.Clear();
        txtNombre.Clear();
        cmbCategoria.SelectedIndex = 0;
        numPrecio.Value = 0;
        numExistencia.Value = 0;
        txtCodigo.Focus();
    }

    private void Info(string mensaje)
    {
        MessageBox.Show(mensaje, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void InitializeComponent()
    {

    }

    private void Error(string mensaje)
    {
        MessageBox.Show(mensaje, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
