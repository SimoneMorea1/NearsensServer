using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nearsens.Models;
using System.Data.SqlClient;

namespace Nearsens.DataAccess
{
    public class SqlPlacesRepository
    {
        string connectionString;

        public SqlPlacesRepository()
            : this("nearsensCS")
        {
        }

        public SqlPlacesRepository(string connectionStringName)
        {
            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cs == null)
                throw new ApplicationException(string.Format("ConnectionString '{0}' not found", connectionStringName));
            connectionString = cs.ConnectionString;
        }

        public GetPlaceQuery GetPlaceById(long id)
        {
            GetPlaceQuery place = new GetPlaceQuery();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
SELECT  id ,
		name ,
        description ,
		main_category ,
		subcategory ,
		lat ,
		lng ,
        icon
      
FROM    dbo.places
WHERE id = @id
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            place.Id = (long)reader["id"];
                            place.Name = (string)reader["name"];
                            place.Description = (string)reader["description"];
                            place.MainCategory = (string)reader["main_category"];
                            place.Subcategory = (string)reader["subcategory"];
                            place.Lat = (double)reader["lat"];
                            place.Lng = (double)reader["lng"];
                            place.Icon = reader["icon"] == DBNull.Value ? (string)null : (string)reader["icon"];

                        }
                    }
                }
            }
            place.Photos = GetPlacePhotos(id);
            return place;
        }

        public IEnumerable<string> GetPlacePhotos(long id)
        {
            List<string> photos = new List<string>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
SELECT  photo 
      
FROM    dbo.photos
WHERE id_place = @id
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var photo = (string)reader["photo"];
                            photos.Add(photo);
                        }
                    }
                }
            }
            return photos;
        }

        public string GetIdUserOfThePlace(long idPlace)
        {
            string id = "";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
SELECT  user_id
FROM    dbo.places
WHERE id = @id
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", idPlace));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id = (string)reader["user_id"];
                        }
                    }
                }
            }
            return id;
        }
        public IEnumerable<GetPlacesByUserIdQuery> GetPlacesByUserId(string id)
        {
            List<GetPlacesByUserIdQuery> places = new List<GetPlacesByUserIdQuery>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
SELECT  id ,
		name ,
        icon
      
FROM    dbo.places
WHERE user_id = @id
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GetPlacesByUserIdQuery place = new GetPlacesByUserIdQuery();
                            place.Id = (long)reader["id"];
                            place.Name = (string)reader["name"];
                            place.Icon = reader["icon"] == DBNull.Value ? (string)null : (string)reader["icon"];
                            places.Add(place);
                        }
                    }
                }
            }
            return places;
        }

        public IEnumerable<GetNearestPlacesQuery> GetNearestPlacesWithFilters(double lat, double lng, int? distanceLimit, string category, string subcategory)
        {
            List<GetNearestPlacesQuery> places = new List<GetNearestPlacesQuery>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
SELECT  id ,
		name ,
		main_category ,
		subcategory ,
		lat ,
		lng ,
        icon
      
FROM    dbo.places
";
                query = BuildWhereClause(query, category, subcategory);
                using (var command = new SqlCommand(query, connection))
                {
                    if (category != null) command.Parameters.Add(new SqlParameter("@category", category));
                    if (subcategory != null) command.Parameters.Add(new SqlParameter("@subcategory", subcategory));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GetNearestPlacesQuery place = new GetNearestPlacesQuery();
                            place.Id = (long)reader["id"];
                            place.Name = (string)reader["name"];
                            place.MainCategory = (string)reader["main_category"];
                            place.Subcategory = (string)reader["subcategory"];
                            place.Lat = (double)reader["lat"];
                            place.Lng = (double)reader["lng"];
                            place.Icon = reader["icon"] == DBNull.Value ? (string)null : (string)reader["icon"];

                            places.Add(place);
                        }
                    }
                }
            }

            var orderedList = places.OrderBy(xx => Utilities.GeoUtilities.CalculateDistance(xx.Lat, lat, xx.Lng, lng));
            if (distanceLimit != null)
                return orderedList.Where(xx => Utilities.GeoUtilities.CalculateDistance(xx.Lat, lat, xx.Lng, lng) < distanceLimit);
            return orderedList;
        }

        public long InsertPlace(Place place)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
INSERT INTO [dbo].[places]
		   ([name]
		   ,[description]
		   ,[lat]
		   ,[lng]
           ,[main_category]
           ,[subcategory]
           ,[user_id])
    OUTPUT INSERTED.ID
	 VALUES
		   (@name
		   ,@description
		   ,@lat
		   ,@lng
		   ,@main_category
           ,@subcategory
           ,@user_id)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@name", place.Name));
                    command.Parameters.Add(new SqlParameter("@description", place.Description));
                    command.Parameters.Add(new SqlParameter("@lat", place.Lat));
                    command.Parameters.Add(new SqlParameter("@lng", place.Lng));
                    command.Parameters.Add(new SqlParameter("@main_category", place.MainCategory));
                    command.Parameters.Add(new SqlParameter("@subcategory", place.Subcategory));
                    command.Parameters.Add(new SqlParameter("@user_id", place.UserId));

                    return (long) command.ExecuteScalar();
                }
            }
        }

        public void InsertIcon(long placeId, string path)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
UPDATE [dbo].[places]
SET [icon] = @path
WHERE [id] = @placeId
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@path", path));
                    command.Parameters.Add(new SqlParameter("@placeId", placeId));
                    
                    int count = command.ExecuteNonQuery();
                }
            }
        }

        public void InsertPlacePhotos(long placeId, List<string> paths)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
INSERT INTO [dbo].[photos]
		   ([id_place]
		   ,[photo])
";

                query = BuildValuesClause(query, paths.Count);

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id_place", placeId));
                    for (int i = 0; i < paths.Count; i++)
                    {
                        command.Parameters.Add(new SqlParameter("@path" + i, paths[i]));
                    }

                    int count = command.ExecuteNonQuery();
                }
            }
        }


        public void DeletePlace(long id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
DELETE FROM [dbo].[photos]
WHERE id_place = @id;

DELETE FROM [dbo].[places]
WHERE id = @id;";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));

                    int count = command.ExecuteNonQuery();
                }
            }
        }
        public void UpdatePlace(Place place)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
UPDATE [dbo].[places]
   SET [description] = @description
	  ,[name] = @name
	  ,[main_category] = @main_category
	  ,[subcategory] = @subcategory
	  ,[lat] = @lat
	  ,[lng] = @lng
	  ,[icon] = @icon
 WHERE id = @id
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", place.Id));
                    command.Parameters.Add(new SqlParameter("@description", place.Description));
                    command.Parameters.Add(new SqlParameter("@name", (object)place.Name ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@main_category", (object)place.MainCategory ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@subcategory", (object)place.Subcategory ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@lat", (object)place.Lat ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@lng", (object)place.Lng ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@icon", (object)place.Icon ?? DBNull.Value));
                    int count = command.ExecuteNonQuery();
                }
            }
        }

        private string BuildWhereClause(string query, string category, string subcategory)
        {
            if (category == null && subcategory == null)
                return query;
            query += " WHERE ";
            if (category != null)
                query += "main_category = @category AND ";
            if (subcategory != null)
                query += "subcategory = @subcategory AND ";
            
            return query.Remove(query.LastIndexOf("AND"));

        }

        private string BuildValuesClause(string query, int pathsCount)
        {
            query += "VALUES";
            for (int i = 0; i < pathsCount; i++)
            {
                query += "(@id_place, @path" + i + "),";
            }

            return query.Remove(query.LastIndexOf(","));
        }
    }
}
