namespace InventarioTienda;


/// NODO de la lista enlazada: guarda un dato (Movimiento)
/// y un enlace al siguiente nodo.


public class NodoMovimiento
{
    public Movimiento Dato;          // La información que guarda el nodo
    public NodoMovimiento Siguiente; // Enlace al próximo nodo (null si es el último)

    public NodoMovimiento(Movimiento dato)
    {
        Dato = dato;
        Siguiente = null;
    }
}


/// lista simple implementada desde cero
/// Se usa para guardar el historial de movimientos del inventario.
/// Operaciones: insertar, buscar, eliminar y recorrer.

public class ListaMovimientos
{
    // "cabeza" apunta al primer nodo. Si es null, la lista está vacía.
    private NodoMovimiento cabeza;

    // Cantidad de nodos que tiene la lista
    public int Cantidad { get; private set; }

    // pregunta si la lista está vacía
    public bool EstaVacia => cabeza == null;

   
    ///agrega un movimiento AL INICIO de la lista.
    /// Así el movimiento más reciente siempre aparece primero al recorrer.
   
    public void InsertarAlInicio(Movimiento movimiento)
    {
        NodoMovimiento nuevo = new NodoMovimiento(movimiento);
        nuevo.Siguiente = cabeza;  // El nuevo nodo apunta al que era el primero
        cabeza = nuevo;            // Ahora el nuevo nodo es la cabeza
        Cantidad++;
    }


    /// BÚSQUEDA: devuelve todos los movimientos de un producto (por su código).
    /// Recorre nodo por nodo comparando el código.
    
    public List<Movimiento> BuscarPorCodigo(string codigo)
    {
        List<Movimiento> encontrados = new List<Movimiento>();
        NodoMovimiento actual = cabeza;

        while (actual != null)
        {
            if (string.Equals(actual.Dato.CodigoProducto, codigo, StringComparison.OrdinalIgnoreCase))
                encontrados.Add(actual.Dato);

            actual = actual.Siguiente; // Avanza al siguiente nodo
        }
        return encontrados;
    }

    
    /// borra el movimiento con el Id indicado.
    /// Devuelve true si lo encontró y lo eliminó.
    /// Mantiene la integridad de la lista: une el nodo eliminado con el de la par.

    public bool EliminarPorId(int id)
    {
        if (cabeza == null) return false; // Lista vacía: no hay nada que eliminar

        // Caso 1: el nodo a eliminar es la cabeza
        if (cabeza.Dato.Id == id)
        {
            cabeza = cabeza.Siguiente; // La cabeza pasa al segundo nodo
            Cantidad--;
            return true;
        }

        // Caso 2: el nodo está en medio o al final.
        // Nos paramos en el nodo anterior al que queremos borrar.
        NodoMovimiento actual = cabeza;
        while (actual.Siguiente != null)
        {
            if (actual.Siguiente.Dato.Id == id)
            {
                // El anterior se enlaza con el siguiente del eliminado
                actual.Siguiente = actual.Siguiente.Siguiente;
                Cantidad--;
                return true;
            }
            actual = actual.Siguiente;
        }
        return false; // No se encontró
    }

   
    ///vissita todos los nodos desde la cabeza hasta el final
    /// y devuelve los movimientos en ese orden (del más reciente al más antiguo).
  
    public List<Movimiento> Recorrer()
    {
        List<Movimiento> todos = new List<Movimiento>();
        NodoMovimiento actual = cabeza;

        while (actual != null)
        {
            todos.Add(actual.Dato);
            actual = actual.Siguiente;
        }
        return todos;
    }
}
