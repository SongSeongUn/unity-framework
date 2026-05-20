using UnityEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Table;


public static class DataTableManager
{
    private static Dictionary<Type, TableReader> _tableCache = new();


   
    private static T GetTable<T>() where T : TableReader, new()
    {
        if(_tableCache.TryGetValue(typeof(T), out var table))
            return table as T;
        else
            return null;    
    }
    
    
    public static async UniTask LoadDataTable(CancellationToken ct = default)
    {
        var tablesToLoad = new List<TableReader>()
        {
            //Table
        };

        try
        {
            // 모든 테이블을 병렬(Parallel)로 동시에 로드
            await UniTask.WhenAll(tablesToLoad.Select(t => t.InitializeAsync(ct)));
        }
        catch(OperationCanceledException)
        {
            DebugUtils.LogError("테이블 로드가 취소되었습니다.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static T GetTableInternal<T>() where T : TableReader, new()
    {
        if (_tableCache.TryGetValue(typeof(T), out var table)) return table as T;
        T newTbl = new T();
        _tableCache.Add(typeof(T), newTbl);
        return newTbl;
    }
}