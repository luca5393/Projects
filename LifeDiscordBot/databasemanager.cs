using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections;

namespace LifeDiscordBot
{

    public class DatabaseManager
    {
        private readonly string connectionString;

        public DatabaseManager()
        {
            connectionString = $"Data Source=minint-dv3pmdb;Initial Catalog=lifediscord;Integrated Security=True;";
        }
        
        public void CreateUserProfile(ulong id, string username, string profileurl)
        {
            try
            {
                using SqlConnection connection = new(connectionString);

                connection.Open();

                using SqlCommand command = connection.CreateCommand();

                command.CommandText = @"INSERT INTO [dbo].[Users] ([id], [username], [profileurl], [money], [location], [workplace],  [cut], [time])
                                    VALUES (@Id, @Username, @Profile, @Money, @Location, @Workplace, @Cut, @Time)
                                    ";
                command.Parameters.AddWithValue("@Id", unchecked((long)id));
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Profile", profileurl);
                command.Parameters.AddWithValue("@Money", 0);
                command.Parameters.AddWithValue("@Location", "Town square");
                command.Parameters.AddWithValue("@Workplace", DBNull.Value);
                command.Parameters.AddWithValue("@Cut", 0);
                command.Parameters.AddWithValue("@Time", DateTime.Now);

                command.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: "+ex);
            }
            
        }
        public bool Userexists(ulong id)
        {
            using SqlConnection connection = new(connectionString);

            connection.Open();

            using SqlCommand command = connection.CreateCommand();


            command.CommandText = @"SELECT * FROM [dbo].[Users] WHERE [id] = @id;";
            command.Parameters.AddWithValue("@id", unchecked((long)id));

            SqlDataReader reader = command.ExecuteReader();

            bool userexists = reader.HasRows;

            connection.Close();

            return userexists;

        }
        public User Userget(ulong id)
        {
            try
            {
                using SqlConnection connection = new(connectionString);

                connection.Open();

                using SqlCommand command = connection.CreateCommand();

                command.CommandText = @"SELECT * FROM [dbo].[Users] WHERE [id] = @id;";
                command.Parameters.AddWithValue("@id", unchecked((long)id));

                SqlDataReader reader = command.ExecuteReader();

                bool userexists = reader.HasRows;

                reader.Read();

                User user = new User
                {
                    id = Convert.ToUInt64(reader["id"]),
                    username = reader["username"].ToString(),
                    profileurl = reader["profileurl"].ToString(),
                    money = Convert.ToInt32(reader["money"]),
                    location = reader["location"].ToString(),
                    workplace = reader["workplace"].ToString(),
                    cut = Convert.ToInt32(reader["cut"]),
                    time = Convert.ToDateTime(reader["time"])

                };

                connection.Close();

                return user;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return null;
            }
        }

        public void Userupdate(User user)
        {
            try
            {
                using SqlConnection connection = new(connectionString);

                connection.Open();

                using SqlCommand command = connection.CreateCommand();


                command.CommandText = "UPDATE Users SET username = @username, profileurl = @profileurl, money = @money, location = @location, workplace = @workplace, cut = @cut, time = @time WHERE id = @id";

                command.Parameters.AddWithValue("@username", user.username);
                command.Parameters.AddWithValue("@profileurl", user.profileurl);
                command.Parameters.AddWithValue("@money", user.money);
                command.Parameters.AddWithValue("@location", user.location);
                if(user.workplace == null)
                {
                    command.Parameters.AddWithValue("@workplace", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@workplace", user.workplace);
                }
                command.Parameters.AddWithValue("@cut", user.cut);
                command.Parameters.AddWithValue("@time", user.time);
                command.Parameters.AddWithValue("@id", unchecked((long)user.id));


                command.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Createcompany(string name, ulong owner, string type, int worth, int operatingcost, int  earnings)
        {
            using SqlConnection connection = new(connectionString);

            connection.Open();

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = @"INSERT INTO [dbo].[Companies] ([name], [owner], [type], [worth], [npcworkers], [playerworkers], [factories], [operatingcost], [earnings])
             VALUES (@Name, @Owner, @Type, @Worth, @NPCWorkers, @PlayerWorkers, @Factories, @OperatingCost, @Earnings)";

            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Owner", unchecked((long)owner));  // Assuming 'owner' is a variable
            command.Parameters.AddWithValue("@Type", type);  // Replace "YourType" with the actual type
            command.Parameters.AddWithValue("@Worth", worth);
            command.Parameters.AddWithValue("@NPCWorkers", 0);  // Assuming default value
            command.Parameters.AddWithValue("@PlayerWorkers", 0);  // Assuming default value
            command.Parameters.AddWithValue("@Factories", 1);  // Assuming default value
            command.Parameters.AddWithValue("@OperatingCost", operatingcost);  // Assuming default value
            command.Parameters.AddWithValue("@Earnings", earnings);  // Assuming default value


            command.ExecuteNonQuery();

            connection.Close();

        }
        public bool companyexists(string name)
        {
            using SqlConnection connection = new(connectionString);

            connection.Open();

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = @"SELECT * FROM [dbo].[Companies] WHERE [name] = @name;";
            command.Parameters.AddWithValue("@name", name);

            SqlDataReader reader = command.ExecuteReader();

            bool userexists = reader.HasRows;

            connection.Close();

            return userexists;

        }
        public Company companyget(string name)
        {
            using SqlConnection connection = new(connectionString);

            connection.Open();

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = @"SELECT * FROM [dbo].[Companies] WHERE [name] = @name;";
            command.Parameters.AddWithValue("@name", name);

            SqlDataReader reader = command.ExecuteReader();

            bool userexists = reader.HasRows;

            reader.Read();

            Company company = new Company
            {
                name = reader["name"].ToString(),
                owner = Convert.ToUInt64(reader["owner"]),
                type = reader["type"].ToString(),
                worth = Convert.ToInt32(reader["worth"]),
                npcworkers = Convert.ToInt32(reader["npcworkers"]),
                playerworkers = Convert.ToInt32(reader["playerworkers"]),
                factories = Convert.ToInt32(reader["factories"]),
                operatingcost = Convert.ToInt32(reader["operatingcost"]),
                earnings = Convert.ToInt32(reader["earnings"])
            };


            connection.Close();

            return company;

        }
        public List<Company> companygetofuser(ulong id)
        {
            List<Company> companies = new List<Company>();

            using SqlConnection connection = new(connectionString);

            connection.Open();

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = @"SELECT * FROM [dbo].[Companies] WHERE [owner] = @id;";
            command.Parameters.AddWithValue("@id", unchecked((long)id));

            SqlDataReader reader = command.ExecuteReader();


            while (reader.Read())
            {
                Company company = new Company
                {
                    name = reader["name"].ToString(),
                    owner = Convert.ToUInt64(reader["owner"]),
                    type = reader["type"].ToString(),
                    worth = Convert.ToInt32(reader["worth"]),
                    npcworkers = Convert.ToInt32(reader["npcworkers"]),
                    playerworkers = Convert.ToInt32(reader["playerworkers"]),
                    stock = Convert.ToInt32(reader["stock"]),
                    stockprice = Convert.ToInt32(reader["stockprice"]),
                    stockname = reader["stockname"].ToString(),
                    factories = Convert.ToInt32(reader["factories"]),
                    operatingcost = Convert.ToInt32(reader["operatingcost"]),
                    earnings = Convert.ToInt32(reader["earnings"])
                };
                companies.Add(company);

            }

            connection.Close();
            return companies;

        }
        public void CompanyUpdate(Company company)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);

                connection.Open();

                using SqlCommand command = connection.CreateCommand();

                command.CommandText = @"UPDATE [dbo].[Companies] SET owner = @owner, type = @type, worth = @worth, npcworkers = @npcworkers, playerworkers = @playerworkers, 
                            factories = @factories, operatingcost = @operatingcost, 
                            earnings = @earnings WHERE name = @name";

                command.Parameters.AddWithValue("@owner", unchecked((long)company.owner));
                command.Parameters.AddWithValue("@type", company.type);
                command.Parameters.AddWithValue("@worth", company.worth);
                command.Parameters.AddWithValue("@npcworkers", company.npcworkers);
                command.Parameters.AddWithValue("@playerworkers", company.playerworkers);
                command.Parameters.AddWithValue("@factories", company.factories);
                command.Parameters.AddWithValue("@operatingcost", company.operatingcost);
                command.Parameters.AddWithValue("@earnings", company.earnings);
                command.Parameters.AddWithValue("@name", company.name);

                command.ExecuteNonQuery();

                connection.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void Createworkapply(ulong id, string name)
        {
            try
            {
                using SqlConnection connection = new(connectionString);

                connection.Open();

                using SqlCommand command = connection.CreateCommand();

                command.CommandText = @"INSERT INTO [dbo].[WorkApply] ([id], [companyname])
                                    VALUES (@Id, @Name)
                                    ";
                command.Parameters.AddWithValue("@Id", unchecked((long)id));
                command.Parameters.AddWithValue("@Name", name);

                command.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: " + ex);
            }

        }
        public bool applyexists(ulong id, string name)
        {
            try
            {
                using SqlConnection connection = new(connectionString);

                connection.Open();

                using SqlCommand command = connection.CreateCommand();


                command.CommandText = @"SELECT * FROM [dbo].[WorkApply] WHERE [id] = @id AND [companyname] = @name;";
                command.Parameters.AddWithValue("@id", unchecked((long)id));
                command.Parameters.AddWithValue("@name", name);

                SqlDataReader reader = command.ExecuteReader();

                bool userexists = reader.HasRows;

                connection.Close();

                return userexists;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }


        }
        public void applydelete(ulong id, string name)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);

                connection.Open();

                using SqlCommand command = connection.CreateCommand();

                command.CommandText = @"DELETE FROM [dbo].[WorkApply] WHERE [id] = @id AND [companyname] = @name;";
                command.Parameters.AddWithValue("@id", unchecked((long)id));
                command.Parameters.AddWithValue("@name", name);

                command.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }

    }
}
