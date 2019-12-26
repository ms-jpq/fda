namespace DotNetExtensions


open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open System.Collections.Generic


module Exts =

    let metadata (req: HttpRequest) =
        let headers =
            req.Headers
            |> Seq.cast<KeyValuePair<string, StringValues>>
            |> Seq.map (fun x -> x.Key, x.Value)
            |> Map.ofSeq

        let cookies =
            req.Cookies
            |> Seq.cast<KeyValuePair<string, string>>
            |> Seq.map (fun x -> x.Key, x.Value)
            |> Map.ofSeq

        headers, cookies
