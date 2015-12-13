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
        #region Initialize

        public void Initialize()
        {
            CreateDatabaseIfNotExists();
            SeedDefaultData();
        }

        private void CreateDatabaseIfNotExists()
        {
            using (var connection = OpenMasterDbConnection())
            {
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

        private bool CheckDbExists(string dbname, SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = '" + dbname + "' OR name = '" + dbname + "')";

                int result = (int)command.ExecuteScalar();

                return result == 1;
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
            using (var connection = OpenDbConnection())
            {
                if (GetStationsCount(connection) == 0)
                {
                    InsertStation(new Station { Name = "Москва Ленинградская" }, connection);
                    InsertStation(new Station { Name = "Москва Казанская" }, connection);
                    InsertStation(new Station { Name = "Москва Павелецкая" }, connection);
                    InsertStation(new Station { Name = "Санкт-Петербург Ладож." }, connection);
                }
            }
        }

        #endregion

        #region Stations

        internal Station GetStation(int id)
        {
            using (var connection = OpenDbConnection())
            {
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

        public void DeleteStation(int id)
        {
            DeleteEntry(id, "Stations");
        }

        internal void UpdateStation(Station station)
        {
            using (var connection = OpenDbConnection())
            {
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

        public void InsertStation(Station station)
        {
            using (var connection = OpenDbConnection())
            {
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
            using (var connection = OpenDbConnection())
            {
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

        #endregion

        #region Trips

        public List<TrainTrip> GetTrips()
        {
            var res = new List<TrainTrip>();

            using (var connection = OpenDbConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT 
                        t.Id            as Id, 
                        t.DepartureTime as DepartureTime, 
                        t.ArrivalTime   as ArrivalTime,
                        sd.Id           as DepartureStationId,
                        sd.Name         as DepartureStationName,
                        sa.Id           as ArrivalStationId,
                        sa.Name         as ArrivalStationName
                    FROM TrainTrips t
                    JOIN Stations sd ON sd.Id = t.DepartureStationId
                    JOIN Stations sa ON sa.Id = t.ArrivalStationId";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(ReadTrip(reader));
                    }
                }
            }

            return res;
        }

        internal TrainTrip GetTrip(int id)
        {
            TrainTrip res = null;

            using (var connection = OpenDbConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT 
                        t.Id            as Id, 
                        t.DepartureTime as DepartureTime, 
                        t.ArrivalTime   as ArrivalTime,
                        sd.Id           as DepartureStationId,
                        sd.Name         as DepartureStationName,
                        sa.Id           as ArrivalStationId,
                        sa.Name         as ArrivalStationName
                    FROM TrainTrips t
                    JOIN Stations sd ON sd.Id = t.DepartureStationId
                    JOIN Stations sa ON sa.Id = t.ArrivalStationId
                    WHERE t.Id = @id";

                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res = ReadTrip(reader);
                    }
                }
            }

            return res;
        }

        internal void UpdateTrip(TrainTrip trip)
        {
            using (var connection = OpenDbConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE TrainTrips
                    SET 
                        DepartureStationId = @did, 
                        DepartureTime = @dt, 
                        ArrivalStationId = @aid, 
                        ArrivalTime = @at) 
                    WHERE Id = @id";

                command.Parameters.AddWithValue("@id", trip.Id);
                command.Parameters.AddWithValue("@did", trip.DepartureStation.Id);
                command.Parameters.AddWithValue("@dt", trip.DepartureTime);
                command.Parameters.AddWithValue("@aid", trip.ArrivalStation.Id);
                command.Parameters.AddWithValue("@at", trip.ArrivalTime);

                command.ExecuteNonQuery();
            }
        }

        internal void InsertTrip(TrainTrip trip)
        {
            using (var connection = OpenDbConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO TrainTrips(
                        DepartureStationId, 
                        DepartureTime, 
                        ArrivalStationId, 
                        ArrivalTime) 
                    OUTPUT INSERTED.Id 
                    VALUES(@did, @dt, @aid, @at)";

                command.Parameters.AddWithValue("@did", trip.DepartureStation.Id);
                command.Parameters.AddWithValue("@dt", trip.DepartureTime);
                command.Parameters.AddWithValue("@aid", trip.ArrivalStation.Id);
                command.Parameters.AddWithValue("@at", trip.ArrivalTime);

                trip.Id = (int)command.ExecuteScalar();
            }
        }

        internal void DeleteTrip(int id)
        {
            DeleteEntry(id, "TrainTrips");
        }

        private TrainTrip ReadTrip(SqlDataReader reader)
        {
            var trip = new TrainTrip();

            trip.Id = (int)reader["Id"];
            trip.DepartureTime = (DateTime)reader["DepartureTime"];
            trip.ArrivalTime = (DateTime)reader["ArrivalTime"];
            trip.DepartureStation = new Station();
            trip.DepartureStation.Id = (int)reader["DepartureStationId"];
            trip.DepartureStation.Name = (string)reader["DepartureStationName"];
            trip.ArrivalStation = new Station();
            trip.ArrivalStation.Id = (int)reader["ArrivalStationId"];
            trip.ArrivalStation.Name = (string)reader["ArrivalStationName"];

            return trip;
        }

        private string GetSelectAllTripsSql()
        {
            return @"
                SELECT 
                    t.Id            as Id, 
                    t.DepartureTime as DepartureTime, 
                    t.ArrivalTime   as ArrivalTime,
                    sd.Id           as DepartureStationId,
                    sd.Name         as DepartureStationName,
                    sa.Id           as ArrivalStationId,
                    sa.Name         as ArrivalStationName
                FROM TrainTrips t
                JOIN Stations sd ON sd.Id = t.DepartureStationId
                JOIN Stations sa ON sa.Id = t.ArrivalStationId";
        }

        #endregion

        #region Common
        
        private void DeleteEntry(int id, string tableName)
        {
            using (var connection = OpenDbConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM " + tableName + " WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        private SqlConnection OpenDbConnection()
        {
            var connection = new SqlConnection(GetConnectionString());
            connection.Open();
            return connection;
        }

        private SqlConnection OpenMasterDbConnection()
        {
            var connection = new SqlConnection(GetMasterDbConnectionString());
            connection.Open();
            return connection;
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

        #endregion
    }
}