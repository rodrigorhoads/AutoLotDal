using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using AutoLotDal.Models;

namespace AutoLotDal.DataOperations
{
    public class InventoryDAL
    {
        private readonly string _connectionString;        

        public InventoryDAL():this(@"Data Source = (localdb)\mssqllocaldb;Integrated Security=true;Initial Catalog=AutoLot"){}

        public InventoryDAL(string connectionString) => _connectionString = connectionString;

        private SqlConnection _sqlConnection = null;
        private void OpenConnection()
        {
            _sqlConnection = new SqlConnection { ConnectionString = _connectionString };
            _sqlConnection.Open();
        }
        private void CloseConnection()
        {
            if (_sqlConnection?.State!=ConnectionState.Closed)
            {
                _sqlConnection.Close();
            }
        }

        public List<Car> GetAllInventory()
        {
            OpenConnection();
            //isso vai segurar as gravações
            List<Car> inventory = new List<Car>();

            //prepara o objeto command
            string sql = "Select * from Inventory";
            using (SqlCommand command = new SqlCommand(sql,_sqlConnection))
            {
                command.CommandType = CommandType.Text;
                SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (dataReader.Read())
                {
                    inventory.Add(new Car {
                        CarId = (long)dataReader["CarId"],
                        Color=(string)dataReader["Color"],
                        Make=(string)dataReader["Make"],
                        PetName=(string)dataReader["PetName"]
                    });
                }
                dataReader.Close();
            }
            return inventory;
        }

        public Car GetCar(long id) {
            OpenConnection();
            Car car = null;
            string sql = $"Select * from Inventory where CarId={id}";
            using (SqlCommand command= new SqlCommand(sql,_sqlConnection))
            {
                command.CommandType = CommandType.Text;
                SqlDataReader datareader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (datareader.Read())
                {
                    car = new Car {
                        CarId = (int)datareader["CarId"],
                        Color = (string)datareader["Color"],
                        Make = (string)datareader["Make"],
                        PetName=(string)datareader["PetName"]
                    };
                }
                datareader.Close();
            }
            return car;
        }

        public void InsertAuto(string colo,string make,string petname)
        {
            OpenConnection();
            //formata e executa a declaração sql
            string sql = $"Insert Into Inventory(Make,Color,PetName) Values ('{make}','{colo}',{petname})";

            //executa usando a conexao
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
            CloseConnection();
        }

        //metodo de inserção utilizando um objeto ou seja fortemente tipado
        //porem os parametros estão hard-coded
        //public void InsertAuto(Car car)
        //{
        //    OpenConnection();
        //    //formata e executa a declaração sql
        //    string sql = $"Insert Into Inventory(Make,Color,PetName) Values ('{car.Make}','{car.Color}',{car.PetName})";
        //    //executa usando a conexao
        //    using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
        //    {
        //        command.CommandType = CommandType.Text;
        //        command.ExecuteNonQuery();
        //    }
        //    CloseConnection();
        //}

        public void DeleteCar(long id)
        {
            OpenConnection();

            //obtem ID do carro para , entao faz
            string sql = $"Delete from Inventory where CarId='{id}'";
            using (SqlCommand command= new SqlCommand(sql,_sqlConnection))
            {
                try
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Exception error = new Exception("Desculpe! Esse carro ja era",ex);
                    throw error;
                }
            }
            CloseConnection();

        }


        public void UpdateCarPetName(int id,string newPetName)
        {
            OpenConnection();

            //obtem Id do carro para modificar o 
            string sql = $"Update Inventory Set PetName = '{newPetName}','{id}'";

            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.ExecuteNonQuery();
            }
            CloseConnection();
        }


        public void InsertAuto(Car car)
        {
            OpenConnection();

            string sql = "Insert into Inventory" +
                "(Make, Color, PetName) Values" +
                "(@Make,@Color,@PetName)";

            //comando com paramentros internos
            using (SqlCommand command=new SqlCommand(sql,_sqlConnection))
            {
                //preenche a coleção de parametros
                SqlParameter parametro = new SqlParameter {
                        ParameterName="@Make",
                        Value=car.Make,
                        SqlDbType=SqlDbType.Char,
                        Size=10
                };
                command.Parameters.Add(parametro);
                parametro = new SqlParameter
                {
                    ParameterName ="@Color",
                    Value =car.Color,
                    SqlDbType =SqlDbType.Char,
                    Size=10
                };
                command.Parameters.Add(parametro);
                parametro = new SqlParameter
                {
                    ParameterName ="@PetName",
                    Value =car.PetName,
                    SqlDbType =SqlDbType.Char,
                    Size=10
                };
                command.Parameters.Add(parametro);

                command.ExecuteNonQuery();
                CloseConnection();
            }
        }

        //usando storedprocedure
        public string LookUpPetName(long carId)
        {
            OpenConnection();
            string carPetName;

            //estabelecendo nome de stored procedure
            using (SqlCommand command = new SqlCommand("GetPetName",_sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                //colocando parametros
                SqlParameter param = new SqlParameter
                {
                    ParameterName="@carId",
                    SqlDbType=SqlDbType.Int,
                    Value=carId,
                    Direction=ParameterDirection.Input
                };
                command.Parameters.Add(param);

                //saida de parametros
                param = new SqlParameter
                {
                    ParameterName = "@petName",
                    SqlDbType = SqlDbType.Char,
                    Size=10,
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(param);

                //executando a stored procedure
                command.ExecuteNonQuery();

                //retornando o parametro de saida
                carPetName = (string)command.Parameters["@petName"].Value;
                CloseConnection();
            }
            return carPetName;
        }

        public void ProcessCreditRisk(bool throwEx,int custId)
        {
            OpenConnection();

            //Primeiro , procurar no atual banco um cliente com o id
            string fName;
            string lName;

            var cmdSelect = new SqlCommand($"Select * from Customers where CustId = {custId}",_sqlConnection);
            using (var dataReader = cmdSelect.ExecuteReader())
            {
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    fName = (string)dataReader["FirstName"];
                    lName = (string)dataReader["LastName"];
                }
                else
                {
                    CloseConnection();
                    return;
                }

                //Cria objetos command que representam cada passo da operação
                var cmdRemove = new SqlCommand($"Delete from Customer where CustId={custId}",_sqlConnection);

                var cmdInsert = new SqlCommand($"Insert Into CreditRisks (FirstName, LastName) Values ('{fName}','{lName}')",_sqlConnection);

                //we will get this from the connection object
                SqlTransaction tx = null;
                try
                {
                    tx = _sqlConnection.BeginTransaction();

                    //lista os comandos dentro dessa transação 
                    cmdInsert.Transaction = tx;
                    cmdRemove.Transaction = tx;

                    //executa os comandos
                    cmdInsert.ExecuteNonQuery();
                    cmdRemove.ExecuteNonQuery();

                    //simula um erro
                    if (throwEx)
                    {
                        throw new Exception("Desculpe Houve um erro na base de dados");
                    }

                    //Confirma as transações
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    //Qualquer erro vai roll back a transação .Usando o novo condicional operador de acesso para checar por null
                    tx?.Rollback();
                }
                finally
                {
                    CloseConnection();
                }
            }
        }













    }
}
