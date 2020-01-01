namespace DomainAgnostic

open System
open System.IO


module IO =

    let slurp path =
        async {
            try
                let! res = File.ReadAllTextAsync path |> Async.AwaitTask
                return Ok res
            with e -> return Error e
        }

    let spit content path =
        async {
            try
                let! res = File.WriteAllTextAsync(path, content) |> Async.AwaitTask
                return Ok res
            with e -> return Error e
        }
