using NLog;
using System.Data.SqlClient;
using TAUpload.Models;
using TAUpload.Repository.Interface;

namespace TAUpload.Repository
{
    public class GnEntityFilesRepository : IGnEntityFilesRepository
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly string INSERT_TO_GN_ENTITY_FILES_SQL =
            "INSERT INTO [dbo].[GN_ENTITY_FILES] " +
            "([GN_MODULE_ID],[ENTITY_CODE],[ENTITY_KEY],[FILE_NAME],[FILE_PATH],[USER_ID_NAME],[UPLOAD_TIME],[ARCHIVE]) " +
            "VALUES (@DbId,@ObjectType,@EntityKey,@FileName,@DirName,@LoadUserID,GETDATE(),@Archive);";

        private static readonly string UPDATE_SUG_AND_TEUR_SQL =
            "UPDATE [dbo].[GN_ENT_FILE_EXT] " +
            "SET [TEUR1] = @teur, [TEUR4] = @teurMe, [FILE_TYPE_CD] = @fileTypeCd " +
            "WHERE GN_MODULE_ID = @DbId AND ENTITY_CODE = @ObjectType AND ENTITY_KEY = @EntityKey;";

        private static readonly string SELECT_FILE_NAME_AND_PATH_SQL =
            "SELECT [FILE_NAME], [FILE_PATH] FROM [dbo].[GN_ENTITY_FILES] " +
            "WHERE GN_MODULE_ID = @DbId " +
            "AND ENTITY_CODE = @ObjectType " +
            "AND ENTITY_KEY = @EntityKey ";

        private static readonly string DELETE_FILE_SQL =
            "DELETE FROM [dbo].[GN_ENTITY_FILES]" +
            "WHERE GN_MODULE_ID = @DbId " +
            "AND ENTITY_CODE = @ObjectType " +
            "AND ENTITY_KEY = @EntityKey";

        private static readonly string SELECT_TEUR_SQL =
            "SELECT [TEUR4] " +
            "FROM [dbo].[GN_ENT_FILE_EXT] " +
            "WHERE GN_MODULE_ID = @DbId " +
            "AND ENTITY_CODE = @ObjectType " +
            "AND ENTITY_KEY = @EntityKey ";

        public bool FileExistInDB(DownloadDTO dto)
        {
            string connectionString =
            "Data Source=" + dto.SqlServerName +
            ";Initial Catalog=" + dto.SqlDbName +
            ";Integrated Security=true";

            using (var connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                using (var command = new SqlCommand(SELECT_FILE_NAME_AND_PATH_SQL, connection))
                {
                    command.Parameters.AddWithValue("@DbId", dto.DbId);
                    command.Parameters.AddWithValue("@ObjectType", dto.ObjectType);
                    command.Parameters.AddWithValue("@EntityKey", dto.EntityKey);

                    // Open the connection in a try/catch block.
                    // Create and execute the ExecuteNonQuery, writing the result
                    // Returns No of rows changed
                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteNonQuery();
                        if (reader == 0)
                        {
                            logger.Warn($"TAUpload:SaveDB: No rows were found");
                        }
                        logger.Info($"TAUpload:SaveDB: File was found");
                        return true;
                    }
                    catch (SqlException ex)
                    {
                        logger.Error($"TAUpload:SaveDB: ERROR while writing results: {ex}");
                        return false;
                    }
                }
            }
        }
        public void Save(DownloadDTO dto)
        {
            string connectionString =
            "Data Source=" + dto.SqlServerName +
            ";Initial Catalog=" + dto.SqlDbName +
            ";Integrated Security=true";

            if (dto.Overwrite == "YES")
            {
                logger.Info($"TAUpload:SaveDB: Overwrite = {dto.Overwrite}, Deleting all files");
                DeleteAllFiles(dto);
            }
            else if (FileExistInDB(dto))
            {
                logger.Info($"TAUpload:SaveDB: File {dto.FileName} already exist, deleting...");
                DeleteFileFromDB(dto);
            }

            using (var connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                using (var command = new SqlCommand(INSERT_TO_GN_ENTITY_FILES_SQL, connection))
                {
                    command.Parameters.AddWithValue("@DbId", dto.DbId);
                    command.Parameters.AddWithValue("@ObjectType", dto.ObjectType);
                    command.Parameters.AddWithValue("@EntityKey", dto.EntityKey);
                    command.Parameters.AddWithValue("@FileName", dto.FileName);
                    command.Parameters.AddWithValue("@DirName", Path.Combine(dto.PathName, dto.DirName));
                    command.Parameters.AddWithValue("@LoadUserID", dto.LoadUserID);
                    command.Parameters.AddWithValue("@Archive", "N");
                    logger.Info($"TAUpload:SaveDB: Connection params: {command.Parameters}");

                    // Open the connection in a try/catch block.
                    // Create and execute the ExecuteNonQuery, writing the result
                    // Returns No of rows changed
                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteNonQuery();
                        if (reader == 0)
                        {
                            logger.Warn($"TAUpload:SaveDB: No rows were inserted");
                        }
                        logger.Info($"TAUpload:SaveDB: File wrote to DB: {dto.FileName}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"TAUpload:SaveDB: ERROR while writing results: {ex}");
                    }
                }
            }
        }
        public void UpdateTeurAndFileType(DownloadDTO dto)
        {
            string connectionString =
            "Data Source=" + dto.SqlServerName +
            ";Initial Catalog=" + dto.SqlDbName +
            ";Integrated Security=true";

            if (!String.IsNullOrEmpty(dto.Teur) && !String.IsNullOrEmpty(dto.FileTypeCd))
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand(UPDATE_SUG_AND_TEUR_SQL, connection))
                    {
                        command.Parameters.AddWithValue("@DbId", dto.DbId);
                        command.Parameters.AddWithValue("@ObjectType", dto.ObjectType);
                        command.Parameters.AddWithValue("@EntityKey", dto.EntityKey);
                        command.Parameters.Add("@teur", System.Data.SqlDbType.VarChar).Value = dto.Teur;
                        command.Parameters.Add("@teurMe", System.Data.SqlDbType.NVarChar).Value = dto.Teur;
                        command.Parameters.AddWithValue("@fileTypeCd", dto.FileTypeCd);
                        logger.Info($"TAUpload:UpdateTeurAndFileType: Connection params: {command.Parameters}");

                        // Open the connection in a try/catch block.
                        // Create and execute the ExecuteNonQuery, writing the result
                        // Returns No of rows changed
                        try
                        {
                            connection.Open();
                            var reader = command.ExecuteNonQuery();
                            if (reader == 0)
                            {
                                logger.Warn($"TAUpload:UpdateTeurAndFileType: No rows were updated");
                            }
                            logger.Info($"TAUpload:UpdateTeurAndFileType: File wrote to DB: {dto.FileName}");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"TAUpload:UpdateTeurAndFileType: ERROR while writing results: {ex}");
                        }
                    }
                }
            }
        }
        public void DeleteFileFromDB(DeleteDto dto)
        {
            string location;
            if (dto.EntityOnly == "YES")
            {
                int numExt = (dto.FileName.Length - dto.FileName.LastIndexOf('.')) - 1;
                string fileExt = dto.FileName.Substring(dto.FileName.LastIndexOf('.'), numExt + 1);
                location = Path.Combine(dto.PathName, dto.EntityKey + fileExt);
            }
            else
            {
                location = Path.Combine(dto.PathName, dto.EntityKey + '-' + dto.FileName);
            }
            logger.Info($"TAUpload:DeleteFileFromDB: removing the files from path: {location}");
            File.Delete(location);
            logger.Info($"TAUpload:DeleteFileFromDB: File deleted from path: {location}");

            string connectionString =
            "Data Source=" + dto.SqlServerName +
            ";Initial Catalog=" + dto.SqlDbName +
            ";Integrated Security=true";

            using (var connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                using (var command = new SqlCommand(DELETE_FILE_SQL, connection))
                {
                    command.Parameters.AddWithValue("@DbId", dto.DbId);
                    command.Parameters.AddWithValue("@ObjectType", dto.ObjectType);
                    command.Parameters.AddWithValue("@EntityKey", dto.EntityKey);

                    // Open the connection in a try/catch block.
                    // Create and execute the ExecuteNonQuery, writing the result
                    // Returns No of rows changed
                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteNonQuery();
                        if (reader == 0)
                        {
                            logger.Warn($"TAUpload:DeleteFileFromDB: No rows were deleted");
                        }
                        logger.Info($"TAUpload:DeleteFileFromDB: File has been deleted from DB: {dto.FileName}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"TAUpload:DeleteFileFromDB: ERROR while deleting file {dto.FileName}: {ex}");
                    }
                }
            }
        }

        public void DeleteFileFromDB(DownloadDTO dto)
        {
            string location;
            if (dto.EntityOnly == "YES")
            {
                int numExt = (dto.FileName.Length - dto.FileName.LastIndexOf('.')) - 1;
                string fileExt = dto.FileName.Substring(dto.FileName.LastIndexOf('.'), numExt + 1);
                location = Path.Combine(Path.Combine(dto.PathName, dto.DirName), dto.EntityKey + fileExt);
            }
            else
            {
                location = Path.Combine(Path.Combine(dto.PathName, dto.DirName), dto.EntityKey + '-' + dto.FileName);
            }
            logger.Info($"TAUpload:DeleteFileFromDB: removing the files from path: {location}");
            File.Delete(location);
            logger.Info($"TAUpload:DeleteFileFromDB: File deleted from path: {location}");

            string connectionString =
            "Data Source=" + dto.SqlServerName +
            ";Initial Catalog=" + dto.SqlDbName +
            ";Integrated Security=true";

            using (var connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                using (var command = new SqlCommand(DELETE_FILE_SQL, connection))
                {
                    command.Parameters.AddWithValue("@DbId", dto.DbId);
                    command.Parameters.AddWithValue("@ObjectType", dto.ObjectType);
                    command.Parameters.AddWithValue("@EntityKey", dto.EntityKey);

                    // Open the connection in a try/catch block.
                    // Create and execute the ExecuteNonQuery, writing the result
                    // Returns No of rows changed
                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteNonQuery();
                        if (reader == 0)
                        {
                            logger.Warn($"TAUpload:DeleteFileFromDB: No rows were deleted");
                        }
                        logger.Info($"TAUpload:DeleteFileFromDB: File has been deleted from DB: {dto.FileName}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"TAUpload:DeleteFileFromDB: ERROR while deleting file {dto.FileName}: {ex}");
                    }
                }
            }
        }

        public void DeleteAllFiles(DownloadDTO dto)
        {
            string connectionString =
            "Data Source=" + dto.SqlServerName +
            ";Initial Catalog=" + dto.SqlDbName +
            ";Integrated Security=true";

            using (var connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                using (var command = new SqlCommand(SELECT_FILE_NAME_AND_PATH_SQL, connection))
                {
                    command.Parameters.AddWithValue("@DbId", dto.DbId);
                    command.Parameters.AddWithValue("@ObjectType", dto.ObjectType);
                    command.Parameters.AddWithValue("@EntityKey", dto.EntityKey);

                    // Open the connection in a try/catch block.
                    // Create and execute the DataReader, writing the result
                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string Fname, Pname;
                                Fname = (string)reader[0];
                                Pname = (string)reader[1];
                                logger.Info($"TAUpload:DeleteAllFiles: Delete File {Fname}");
                                dto.FileName = Fname;
                                dto.DirName = Pname;
                                DeleteFileFromDB(dto);
                            }
                        }
                        else
                        {
                            logger.Warn($"TAUpload:DeleteAllFiles: No rows were deleted");
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"TAUpload:DeleteAllFiles: ERROR while deleting file {dto.FileName}: {ex}");
                    }
                }
            }
        }

        public string SelectTeur(DownloadDTO dto)
        {
            string connectionString =
            "Data Source=" + dto.SqlServerName +
            ";Initial Catalog=" + dto.SqlDbName +
            ";Integrated Security=true";

            using (var connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                using (var command = new SqlCommand(SELECT_TEUR_SQL, connection))
                {
                    command.Parameters.AddWithValue("@DbId", dto.DbId);
                    command.Parameters.AddWithValue("@ObjectType", dto.ObjectType);
                    command.Parameters.AddWithValue("@EntityKey", dto.EntityKey);

                    // Open the connection in a try/catch block.
                    // Create and execute the DataReader, writing the result
                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteReader();
                        if(reader.HasRows)
                        {
                            while(reader.Read())
                            {
                                return (string)reader[0];
                            }
                        };
                    }
                    catch (SqlException ex)
                    {
                        logger.Error($"TAUpload:SelectTeur: ERROR while writing results: {ex}");
                    }
                }
            }
            return string.Empty;
        }
    }
}
