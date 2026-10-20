namespace InventarioTienda;

/// Representa un producto de la tienda.
/// Estos objetos se guardan en el ArrayList  llamado "catalogo".

public class Producto
{
    public string Codigo { get; set; }       // Identificador único (ej: A001)
    public string Nombre { get; set; }       // Nombre del producto
    public string Categoria { get; set; }    // Una de las categorías del vector "categorias"
    public decimal Precio { get; set; }      // Precio unitario
    public int Existencia { get; set; }      // Unidades disponibles en inventario
}


/// Representa un movimiento del inventario (entrada, salida, registro, etc.).
/// Estos objetos viajan dentro de los nodos de la lista enlazada.

public class Movimiento
{
    public int Id { get; set; }                  // Número consecutivo del movimiento
    public DateTime Fecha { get; set; }          // Cuándo ocurrió
    public string CodigoProducto { get; set; }   // Código del producto afectado
    public string NombreProducto { get; set; }   // Nombre del producto (se guarda por si el producto se elimina)
    public string Tipo { get; set; }             // Registro, Entrada, Salida, Ajuste o Eliminación
    public int Cantidad { get; set; }            // Unidades involucradas

    // Propiedad calculada solo para mostrarla en la tabla
    public string Producto => CodigoProducto + " - " + NombreProducto;
}
