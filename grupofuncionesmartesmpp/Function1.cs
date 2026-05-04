using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace grupofuncionesmartesmpp;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("Function1")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        //VAMOS A RECIBIR UN PARAMETRO CON EL ID DEL EMPLEADO
        string idEmpleado = req.Query["idempleado"];
        if (idEmpleado == null)
        {
            _logger.LogInformation("No tenemos parámetro");
            return new BadRequestObjectResult
                ("Necesitamos el ID del empleado para trabajar...");
        }
        else
        {
            //INCLUIMOS LA CADENA DE CONEXIÓN MANUAL AQUÍ
            //string connectionString = "Data Source=sqlpedroche3430.database.windows.net;Initial Catalog=AZURETAJAMAR;Persist Security Info=True;User ID=adminsql;Password=Admin123;Encrypt=True;Trust Server Certificate=True";
            string connectionString = Environment.GetEnvironmentVariable("SqlAzure");
            string sql = "UPDATE EMP SET SALARIO=SALARIO + 1 "
                + "WHERE EMP_NO=@idempleado";
            SqlConnection cn = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            com.Connection = cn;
            com.CommandType = CommandType.Text;
            com.CommandText = sql;
            await cn.OpenAsync();
            com.Parameters.AddWithValue("@idempleado", int.Parse(idEmpleado));
            await com.ExecuteNonQueryAsync();
            sql = "select * from EMP where EMP_NO=@idempleado";
            com.CommandText = sql;
            SqlDataReader reader = await com.ExecuteReaderAsync();
            if (reader.Read())
            {
                //TENEMOS DATOS
                string mensaje = "El empleado " + reader["APELLIDO"] + " tiene un nuevo salario de " + reader["SALARIO"];
                await reader.CloseAsync();
                await cn.CloseAsync();
                com.Parameters.Clear();
                return new OkObjectResult(mensaje);
            }
            else
            {
                //EL EMPLEADO NO EXISTE
                return new BadRequestObjectResult
                    ("El empleado con ID " + idEmpleado + " no existe");
            }
        }
    }
}