using Nearsens.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nearsens.DataAccess
{
    public class SqlOffersRepository
    {

        
        string connectionString;

        public SqlOffersRepository()
            : this("nearsensCS")
        {
        }

        public SqlOffersRepository(string connectionStringName)
        {
            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cs == null)
                throw new ApplicationException(string.Format("ConnectionString '{0}' not found", connectionStringName));
            connectionString = cs.ConnectionString;
        }
        public IEnumerable<GetOffersByPlaceIdQuery> GetOffersByPlaceId(long id, bool onlyValidOffers)
        {
            List<GetOffersByPlaceIdQuery> offers = new List<GetOffersByPlaceIdQuery>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
SELECT  id ,
		icon ,
		expiration_date ,
		start_date ,
        title ,
        price ,
        discount
FROM    dbo.offers
WHERE id_place = @id
";

                if (onlyValidOffers)
                    query += " AND CAST(GETDATE() as DATE) BETWEEN start_date AND expiration_date";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GetOffersByPlaceIdQuery offer = new GetOffersByPlaceIdQuery();

                            offer.Id = (long)reader["id"];
                            offer.Title = (string)reader["title"];
                            offer.ExpirationDate = (DateTime)reader["expiration_date"];
                            offer.StartDate = (DateTime)reader["start_date"];
                            offer.Icon = reader["icon"] == DBNull.Value ? (string)null : (string)reader["icon"];
                           
                            offers.Add(offer);
                        }
                    }
                }
            }
            return offers;
        }

        public IEnumerable<GetNearestOffersQuery> GetNearestOffers(double lat, double lng, string category, string subcategory, int? distanceLimit)
        {
            List<GetNearestOffersQuery> offers = new List<GetNearestOffersQuery>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
SELECT  dbo.offers.id ,
		title ,
		price ,
		discount ,
        main_photo ,
        name ,
        lat ,
        lng
FROM    dbo.offers, dbo.places
WHERE dbo.offers.id_place = dbo.places.id  AND CAST(GETDATE() as DATE) BETWEEN start_date AND expiration_date
";
                query = BuildWhereClause(query, category, subcategory);
                using (var command = new SqlCommand(query, connection))
                {
                    if (category != null)
                        command.Parameters.Add(new SqlParameter("@category", category));
                    if (subcategory != null)
                        command.Parameters.Add(new SqlParameter("@subcategory", subcategory));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GetNearestOffersQuery offer = new GetNearestOffersQuery();

                            offer.Id = (long)reader["id"];
                            offer.Title = (string)reader["title"];
                            offer.Price = (decimal)reader["price"];
                            offer.Discount = (int)reader["discount"];
                            offer.MainPhoto = reader["main_photo"] == DBNull.Value ? (string)null : (string)reader["main_photo"];
                            offer.PlaceName = (string)reader["name"];
                            offer.PlaceLat = (double)reader["lat"];
                            offer.PlaceLng = (double)reader["lng"];

                            offers.Add(offer);
                        }
                    }
                }
            }

            var orderedList = offers.OrderBy(xx => Utilities.GeoUtilities.CalculateDistance(xx.PlaceLat, lat, xx.PlaceLng, lng));
            if (distanceLimit != null)
                return orderedList.Where(xx => Utilities.GeoUtilities.CalculateDistance(xx.PlaceLat, lat, xx.PlaceLng, lng) < distanceLimit);
            return orderedList;
        }

        public GetOfferQuery GetOfferById(long id)
        {
            GetOfferQuery offer = new GetOfferQuery();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
SELECT  id ,
		title ,
        description ,
		start_date ,
		expiration_date ,
		price ,
		discount ,
        main_photo ,
        id_place,
        link
      
FROM    dbo.offers
WHERE id = @id
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            offer.Id = (long)reader["id"];
                            offer.Title = (string)reader["title"];
                            offer.Description = (string)reader["description"];
                            offer.StartDate = (DateTime)reader["start_date"];
                            offer.ExpirationDate = (DateTime)reader["expiration_date"];
                            offer.Price = (decimal)reader["price"];
                            offer.Discount = (int)reader["discount"];
                            offer.IdPlace = (long)reader["id_place"];
                            offer.MainPhoto = reader["main_photo"] == DBNull.Value ? (string)null : (string)reader["main_photo"];
                            offer.Link = reader["link"] == DBNull.Value ? (string)null : (string)reader["link"];
                        }
                    }
                }
            }
            offer.Photos = GetOfferPathPhotos(id);
            return offer;
        }

        public IEnumerable<OfferPhoto> GetOfferPhotos(long id)
        {
            List<OfferPhoto> offerPhotos = new List<OfferPhoto>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"

SELECT  id,
        photo,
        id_offer 
    
FROM    dbo.photos_offers
WHERE id_offer = @id
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OfferPhoto offerPhoto = new OfferPhoto();

                            offerPhoto.Id = (long)reader["id"];
                            offerPhoto.Path = (string)reader["photo"];
                            offerPhoto.IdOffer = (long)reader["id_offer"];

                            offerPhotos.Add(offerPhoto);
                        }
                    }
                }
            }
            return offerPhotos;
        }


        public IEnumerable<string> GetOfferPathPhotos(long id)
        {
            List<string> photos = new List<string>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

string query = @"

SELECT  photo 
      
FROM    dbo.photos_offers
WHERE id_offer = @id
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
        public void DeletePhotos(int[] listaId)
        {
     
            for( int i = 0 ; i < listaId.Length; i++){

                DeletePhoto(listaId[i]);
            }
        }
        public void DeletePhoto(int id)
        {

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
DELETE FROM [dbo].[photos_offers]
 WHERE id = @id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));

                    int count = command.ExecuteNonQuery();
                }
            }
        }

        public long InsertOffer(Offer offer)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
INSERT INTO [dbo].[offers]
		   ([title]
		   ,[description]
		   ,[start_date]
		   ,[expiration_date]
           ,[price]
           ,[discount]
           ,[link]
           ,[id_place])
    OUTPUT INSERTED.ID
	 VALUES
		   (@title
		   ,@description
		   ,@start_date
		   ,@expiration_date
		   ,@price
           ,@discount
           ,@link
           ,@id_place)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@title", offer.Title));
                    command.Parameters.Add(new SqlParameter("@description", offer.Description));
                    command.Parameters.Add(new SqlParameter("@start_date", offer.StartDate));
                    command.Parameters.Add(new SqlParameter("@expiration_date", offer.ExpirationDate));
                    command.Parameters.Add(new SqlParameter("@price", offer.Price));
                    command.Parameters.Add(new SqlParameter("@discount", offer.Discount));
                    command.Parameters.Add(new SqlParameter("@id_place", offer.IdPlace));
                    command.Parameters.Add(new SqlParameter("@link", offer.Link));

                    return (long) command.ExecuteScalar();
                }
            }
        }

        public void InsertIcon(long offerId, string path)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
UPDATE [dbo].[offers]
SET [icon] = @path
WHERE [id] = @offerId
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@path", path));
                    command.Parameters.Add(new SqlParameter("@offerId", offerId));

                    int count = command.ExecuteNonQuery();
                }
            }
        }

        public void InsertMainPhoto(long offerId, string path)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
UPDATE [dbo].[offers]
SET [main_photo] = @path
WHERE [id] = @offerId
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@path", path));
                    command.Parameters.Add(new SqlParameter("@offerId", offerId));

                    int count = command.ExecuteNonQuery();
                }
            }
        }

        public void InsertOfferPhotos(long offerId, List<string> paths)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
USE [nearsens]

INSERT INTO [dbo].[photos_offers]
		   ([id_offer]
		   ,[photo])
";

                query = BuildValuesClause(query, paths.Count);

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id_offer", offerId));
                    for (int i = 0; i < paths.Count; i++)
                    {
                        command.Parameters.Add(new SqlParameter("@path" + i, paths[i]));
                    }

                    int count = command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteOffer(long id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
DELETE FROM [dbo].[offers]
 WHERE id = @id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));

                    int count = command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteOffersByPlaceId(long placeId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
DELETE FROM [dbo].[offers]
 WHERE id_place = @placeId";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@placeId", placeId));

                    int count = command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateOffer(Offer offer)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
UPDATE [dbo].[offers]
   SET [description] = @description
	  ,[title] = @title
	  ,[price] = @price
	  ,[discount] = @discount
	  ,[start_date] = @start_date
	  ,[expiration_date] = @expiration_date
      ,[link] = @link
 WHERE id = @id
";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", offer.Id));
                    command.Parameters.Add(new SqlParameter("@description", offer.Description));
                    command.Parameters.Add(new SqlParameter("@title", offer.Title));
                    command.Parameters.Add(new SqlParameter("@price", offer.Price));
                    command.Parameters.Add(new SqlParameter("@discount", offer.Discount));
                    command.Parameters.Add(new SqlParameter("@start_date", offer.StartDate));
                    command.Parameters.Add(new SqlParameter("@expiration_date", offer.ExpirationDate));
                    command.Parameters.Add(new SqlParameter("@link", offer.Link));
                    int count = command.ExecuteNonQuery();
                }
            }
        }
        private string BuildWhereClause(string query, string category, string subcategory)
        {
            if (category == null && subcategory == null)
                return query;
            query += " AND ";
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
                query += "(@id_offer, @path" + i + "),";
            }

            return query.Remove(query.LastIndexOf(","));
        }
    }
}
