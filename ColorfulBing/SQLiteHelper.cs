using System;
using Mono.Data.Sqlite;
using ColorfulBing.Model;
using System.Threading.Tasks;

namespace ColorfulBing {
    public static class SQLiteHelper {
        public static string ConnectionString = String.Format("Data Source={0}", Consts.DBFile);
        public const string BDataTableName = "BDataTable";

        private static string CreateBDataTableSql = String.Format("CREATE TABLE IF NOT EXISTS {0} (ID VARCHAR(40) PRIMARY KEY NOT NULL,TITLE VARCHAR(100)," +
            "COPYRIGHT VARCHAR(200),DESCRIPTION VARHCAR(500),LOCATION VARCHAR(100),BITMAP BLOB, CALENDAR INTEGER)", BDataTableName);

        static SQLiteHelper() {
            CreateTableIfNotExist();
        }

        private static void CreateTableIfNotExist() {
            using (var conn = GetConnection()) {
                using (var command = new SqliteCommand(conn)) {
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = CreateBDataTableSql;

                    command.ExecuteNonQuery();
                }
            }
        }

        public static int GetCount() {
            using (var conn = GetConnection()) {
                using (var command = new SqliteCommand(conn)) {
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = String.Format("SELECT COUNT(*) FROM {0}", BDataTableName);
                    var result = command.ExecuteScalar();
                    return int.Parse(result.ToString());
                }
            }
        }

        public static async Task<BData> GetBDataAsync(DateTime dt) {
            using (var conn = GetConnection()) {
                using (var command = new SqliteCommand(conn)) {
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = String.Format("SELECT * FROM {0} WHERE CALENDAR={1}", BDataTableName, DateUtil.GetDateTicks(dt));
                    using (var dr = await command.ExecuteReaderAsync().ConfigureAwait(false)) {
                        if (dr.Read()) {
                            return new BData {
                                Id = dr["Id"].ToString(),
                                Title = dr["Title"].ToString(),
                                Copyright = dr["Copyright"].ToString(),
                                Description = dr["Description"].ToString(),
                                Location = dr["Location"].ToString(),
                                Bitmap = await BitmapUtil.GetBitmapAsync((byte[])dr["Bitmap"]).ConfigureAwait(false),
                                Calendar = DateUtil.GetDateTimeFromDateTicks(long.Parse(dr["Calendar"].ToString()))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public static async void InsertBDataAsync(BData bdata) {
            var entity = await GetBDataAsync(bdata.Calendar);
            if (entity != null) {
                return;
            }
            using (var conn = GetConnection()) {
                using (var command = new SqliteCommand(conn)) {
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = String.Format("INSERT INTO {0} VALUES(:Id,:Title,:Copyright,:Description,:Location,:Bitmap,:Calendar)", BDataTableName);

                    command.Parameters.Add(new SqliteParameter("Id", bdata.Id));
                    command.Parameters.Add(new SqliteParameter("Title", bdata.Title));
                    command.Parameters.Add(new SqliteParameter("Copyright", bdata.Copyright));
                    command.Parameters.Add(new SqliteParameter("Description", bdata.Description));
                    command.Parameters.Add(new SqliteParameter("Location", bdata.Location));
                    command.Parameters.Add(new SqliteParameter("Bitmap", await BitmapUtil.GetBitmapBufferAsync(bdata.Bitmap)));
                    command.Parameters.Add(new SqliteParameter("Calendar", DateUtil.GetDateTicks(bdata.Calendar)));

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private static SqliteConnection GetConnection() {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}