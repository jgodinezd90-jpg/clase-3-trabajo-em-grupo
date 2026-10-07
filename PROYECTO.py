# Aquí vamos guardando los nombres
estudiantes = []

# Aquí guardamos las notas
notas = []

# Preguntamos cuántos estudiantes se van a registrar
cantidad = int(input("¿Cuántos estudiantes desea registrar? "))

# Repetimos el proceso según la cantidad de estudiantes
for i in range(cantidad):

    # Pedimos el nombre
    nombre = input("Nombre del estudiante: ")

    # Pedimos la nota
    nota = float(input("Nota del estudiante: "))

    # Guardamos los datos en las listas
    estudiantes.append(nombre)
    notas.append(nota)

# Mostramos todos los estudiantes con su nota
print("\n--- LISTA DE ESTUDIANTES ---")

for i in range(len(estudiantes)):
    print(estudiantes[i], "-", notas[i])