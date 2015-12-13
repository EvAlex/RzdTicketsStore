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
                        WagonNumber INT NOT NULL,
                        SeatNumber INT NOT NULL,
                        BookingTime DATETIME DEFAULT NULL,
                        CustomerId INT FOREIGN KEY REFERENCES Customers(Id)
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
                    var stations = new Station[]
                        {
                            new Station { Name = "Москва Ленинградская" },
                            new Station { Name = "Москва Казанская" },
                            new Station { Name = "Москва Павелецкая" },
                            new Station { Name = "Санкт-Петербург Ладож." },
                            new Station { Name = "Казань" },
                            new Station { Name = "Воронеж" },
                            new Station { Name = "Нижний Новгород" },
                            new Station { Name = "Адлер" },
                        };
                    foreach (var s in stations)
                    {
                        InsertStation(s);
                    }

                    var trips = new TrainTrip[]
                        {
                            new TrainTrip
                            {
                                DepartureStation = stations[3],
                                ArrivalStation = stations[0],
                                DepartureTime = new DateTime(2015, 12, 10, 0, 11, 0),
                                ArrivalTime = new DateTime(2015, 12, 10, 0, 11, 0)
                            },
                            new TrainTrip
                            {
                                DepartureStation = stations[4],
                                ArrivalStation = stations[1],
                                DepartureTime = new DateTime(2015, 12, 14, 19, 45, 0),
                                ArrivalTime = new DateTime(2015, 12, 15, 7, 10, 0)
                            }
                        };

                    foreach (var t in trips)
                    {
                        InsertTrip(t);
                        InsertTickets(t.Id, wagonsCount: 16, seatsCountPerWagon: 37, cost: 4554);
                    }
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

        public void InsertTrip(TrainTrip trip)
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

        #region Tickets

        public Ticket[] GetTickets(TrainTrip trip)
        {
            var res = new List<Ticket>();

            using (var connection = OpenDbConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT
	                    t.Id          AS TicketId,
	                    t.TripId      AS TicketTripId,
	                    t.Cost        AS TicketCost,
	                    t.BookingTime AS TicketBookingTime,
	                    t.SeatNumber  AS TicketSeatNumber,
	                    t.WagonNumber AS TicketWagonNumber,
	                    c.Id          AS CustomerId,
	                    c.Surname     AS CustomerSurname,
	                    c.Name        AS CustomerName,
	                    c.Fathersname AS CustomerFathersname,
	                    c.BirthDate   AS CustomerBirthdate
                    FROM Tickets t
                    LEFT JOIN Customers c ON c.Id = t.CustomerId 
                    WHERE t.TripId = @id";

                command.Parameters.AddWithValue("@id", trip.Id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(ReadTicket(reader, trip));
                    }
                }
            }

            return res.ToArray();
        }

        public void InsertTickets(int tripId, int wagonsCount, int seatsCountPerWagon, double cost)
        {
            using (var connection = OpenDbConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Tickets (TripId, Cost, WagonNumber, SeatNumber) VALUES ";

                var values = new List<string>();
                for (int w = 0; w < wagonsCount; w++)
                {
                    for (int s = 0; s < seatsCountPerWagon; s++)
                    {
                        values.Add(string.Format("({0}, {1}, {2}, {3})", tripId, cost, w + 1, s + 1));
                    }
                }
                command.CommandText += string.Join(", ", values);

                command.ExecuteNonQuery();
            }
        }

        private Ticket ReadTicket(SqlDataReader reader, TrainTrip trip)
        {
            var res = new Ticket();
            res.Id = (int)reader["TicketId"];
            res.Trip = trip;
            res.Cost = (double)reader["TicketCost"];
            res.BookingTime = reader["TicketBookingTime"] == DBNull.Value ? null : (DateTime?)reader["TicketBookingTime"];
            res.SeatNumber = (int)reader["TicketSeatNumber"];
            res.WagonNumber = (int)reader["TicketWagonNumber"];
            res.Passenger = ReadCustomer(reader);
            return res;
        }

        #endregion

        #region Customers

        private Customer ReadCustomer(SqlDataReader reader)
        {
            if (reader["CustomerId"] == DBNull.Value)
                return null;

            Customer res = new Customer();

            res.Id = (int)reader["CustomerId"];
            res.Surname = (string)reader["CustomerSurname"];
            res.Name = (string)reader["CustomerName"];
            res.Fathersname = (string)reader["CustomerFathersname"];
            res.BirthDate = (DateTime)reader["CustomerBirthdate"];

            return res;
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