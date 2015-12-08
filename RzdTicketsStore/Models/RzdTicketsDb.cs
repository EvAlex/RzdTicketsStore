using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace RzdTicketsStore.Models
{
    public class RzdTicketsDb
    {
        public void Initialize()
        {
            CreateDatabaseIfNotExists();
            SeedDefaultData();
        }

        private void CreateDatabaseIfNotExists()
        {
            using (var connection = new SqlConnection(GetMasterDbConnectionString()))
            {
                connection.Open();

                if (!CheckDbExists(GetDbName(), connection))
                {
                    CreateDatabase(connection);
                    connection.ChangeDatabase(GetDbName());
                    CreateTables(connection);
                }
            }
        }

        private void CreateDatabase(SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE DATABASE " + GetDbName();
                command.ExecuteNonQuery();
            }
        }

        internal Station GetStation(int id)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                return GetStation(id, connection);
            }
        }

        private Station GetStation(int id, SqlConnection connection)
        {
            Station res = null;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Name FROM Stations WHERE Id = Id";
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res = ReadStation(reader);
                    }
                }
            }

            return res;
        }

        private bool CheckDbExists(string dbname, SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = '" + dbname + "' OR name = '" + dbname + "')";

                int result = (int)command.ExecuteScalar();

                return result == 1;
            }
        }

        internal void DeleteStation(int id)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                DeleteStation(id, connection);
            }
        }

        private void DeleteStation(int id, SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Stations WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        internal void UpdateStation(Station station)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                UpdateStation(station, connection);
            }
        }

        private void UpdateStation(Station station, SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE Stations SET Name = @name WHERE Id = @id";
                command.Parameters.AddWithValue("@id", station.Id);
                command.Parameters.AddWithValue("@name", station.Name);
                command.ExecuteNonQuery();
            }
        }

        private void CreateTables(SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE Stations (
                        Id INT IDENTITY(1, 1) PRIMARY KEY,
                        Name NVARCHAR(200)
                    );

                    CREATE TABLE Customers (
                        Id INT IDENTITY(1, 1) PRIMARY KEY,
                        Surname NVARCHAR(50),
                        Name NVARCHAR(50),
                        Fathersname NVARCHAR(50),
                        PassportNumber NVARCHAR(10),
                        BirthDate DATETIME
                    );

                    CREATE TABLE TrainTrips (
                        Id INT IDENTITY(1, 1) PRIMARY KEY,
                        DepartureStationId INT NOT NULL FOREIGN KEY REFERENCES Stations(Id),
                        DepartureTime DATETIME NOT NULL,
                        ArrivalStationId INT NOT NULL FOREIGN KEY REFERENCES Stations(Id),
                        ArrivalTime DATETIME NOT NULL,
                    );
                        
                    CREATE TABLE Tickets (
                        Id INT IDENTITY(1, 1) PRIMARY KEY,
                        TripId INT NOT NULL FOREIGN KEY REFERENCES TrainTrips(Id),
                        Cost FLOAT,
                        BookingTime DATETIME DEFAULT NULL,
                        PassangerId INT FOREIGN KEY REFERENCES Customers(Id)
                    );";
                command.ExecuteNonQuery();
            }
        }

        private void SeedDefaultData()
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                if (GetStationsCount(connection) == 0)
                {
                    InsertStation(new Station { Name = "Москва Ленинградская" }, connection);
                    InsertStation(new Station { Name = "Москва Казанская" }, connection);
                    InsertStation(new Station { Name = "Москва Павелецкая" }, connection);
                    InsertStation(new Station { Name = "Санкт-Петербург Ладож." }, connection);
                }
            }
        }

        private string GetConnectionString()
        {
            return WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private string GetDbName()
        {
            return new SqlConnection(GetConnectionString()).Database;
        }

        private string GetMasterDbConnectionString()
        {
            return WebConfigurationManager.ConnectionStrings["MasterDbConnection"].ConnectionString;
        }

        public void InsertStation(Station station)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                InsertStation(station, connection);
            }
        }

        public void InsertStation(Station station, SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Stations(Name) OUTPUT INSERTED.Id VALUES(@name)";
                command.Parameters.AddWithValue("@name", station.Name);
                station.Id = (int)command.ExecuteScalar();
            }
        }

        public int GetStationsCount(SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Stations";
                return (int)command.ExecuteScalar();
            }
        }

        public List<Station> GetStations()
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                return GetStations(connection);
            }
        }

        public List<Station> GetStations(SqlConnection connection)
        {
            List<Station> res = new List<Station>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Name FROM Stations";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(ReadStation(reader));
                    }
                }
            }
            return res;
        }

        private Station ReadStation(SqlDataReader reader)
        {
            var station = new Station();
            station.Id = reader.GetInt32(0);
            station.Name = reader.GetString(1);
            return station;
        }
    }
}