namespace DotNetExtensions


open DomainAgnostic
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open System.Collections.Generic


module Exts =

    let Metadata(req: HttpRequest) =
        let headers =
            req.Headers
            |> Seq.cast<KeyValuePair<string, StringValues>>
            |> Map.OfKVP

        let cookies =
            req.Cookies
            |> Seq.cast<KeyValuePair<string, string>>
            |> Map.OfKVP

        headers, cookies


    let AddHeaders headers (resp: HttpResponse) =
        headers
        |> Seq.map (fun (k, v: string) -> k, StringValues(v))
        |> Map.ofSeq
        |> Map.ToKVP
        |> Seq.iter resp.Headers.Add
