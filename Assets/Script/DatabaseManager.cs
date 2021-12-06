using Mono.Data.Sqlite;
using System.IO;
using UnityEngine;

//데이터 베이스 접근 및 쿼리 문 함수화
public class DatabaseManager
{
    private SqliteConnection databaseConnection;
    private SqliteCommand databaseCommand;
    private SqliteDataReader reader;


    public DatabaseManager(string databaseFileName)
    {
        OpenDatabase(databaseFileName);
    }

    //<<db 파일의 경로에 접근하여 연결>>
    public void OpenDatabase(string databaseFileName)
    {
        string file_path = Path.Combine(Application.persistentDataPath, databaseFileName);

        if (!File.Exists(file_path))
        {
            //안드로이드 플랫폼일 때 경로 추적
            if (Application.platform == RuntimePlatform.Android)
            {
                //안드로이드에서의 StreamingAssets 경로
                WWW temp_load_db = new WWW("jar:file://" + Application.dataPath
                    + "!/assets/" + databaseFileName);

                //예외 처리
                while (!temp_load_db.isDone) { 
                    Debug.Log("Incorrect file path!");
                    return; 
                };

                //Application.persistentDataPath에 저장
                File.WriteAllBytes(file_path, temp_load_db.bytes);
            }
            //그 외 플랫폼일 때 경로 추적
            else
            {
                File.Copy(Path.Combine(Application.streamingAssetsPath, databaseFileName), file_path);
            }
        }

        string temp_uri_path = "URI=file:" + file_path;

        databaseConnection = new SqliteConnection(temp_uri_path);
        databaseConnection.Open();
        //Debug.Log("Connected to database");
    }

    //<<DB 연결 끊기>>
    public void CloseSqlConnection()
    {
        if (databaseCommand != null)
        {
            databaseCommand.Dispose();
        }

        databaseCommand = null;

        if (reader != null)
        {
            reader.Dispose();
        }

        reader = null;

        if (databaseConnection != null)
        {
            databaseConnection.Close();
        }

        databaseConnection = null;
        //Debug.Log("Disconnected from database.");
    }


    //<<Query문 실행>>
    public SqliteDataReader ExecuteQuery(string sqlQuery)
    {
        databaseCommand = databaseConnection.CreateCommand();
        databaseCommand.CommandText = sqlQuery;

        reader = databaseCommand.ExecuteReader();

        return reader;
    }

    //<<SQL Qeury문 함수>>
    //<<SELECT (option) items FROM tablename WHERE col op values AND ``` (ORDER BY)>>
    public SqliteDataReader SelectWhere(string tableName, string option, string[] items,
                                        string[] col, string[] operation, string[] values,
                                        string orderCol, string type)
    {
        if (col.Length != operation.Length || operation.Length != values.Length)
        {
            throw new SqliteException("col.Length != operatrion.Length != values.Length");
        }

        string query = "SELECT ";
        
        if (option != null)
        {
            query += option + " ";
        }

        query += items[0];

        for (int i = 1; i < items.Length; ++i)
        {
            query += ", " + items[i];
        }

        query += " FROM " + tableName + " WHERE " + col[0] + operation[0] + "'" + values[0] + "' ";

        for (int i = 1; i <col.Length; ++i)
        {
            query += " AND " + col[i] + operation[i] + "'" + values[0] + "' ";
        }

        if (orderCol != null)
        {
            query += "ORDER BY " + orderCol + " " + type;
        }

        return ExecuteQuery(query);
    }
}
