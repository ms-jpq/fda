namespace DotNetExtensions


open DomainAgnostic
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives


module Exts =

    let Ctx(ctx: HttpContext) = ctx.Request, ctx.Response, ctx.Connection

    let Headers(req: HttpRequest) = req.Headers |> Map.OfKVP

    let Cookies(req: HttpRequest) = req.Cookies |> Map.OfKVP

    let Query(req: HttpRequest) = req.Query |> Map.OfKVP

    let Form(req: HttpRequest) =
        req.ReadFormAsync()
        |> Async.AwaitTask
        |> Async.Map Map.OfKVP


    let AddHeaders headers (resp: HttpResponse) =
        headers
        |> Seq.map (fun (k, v: string) -> k, StringValues(v))
        |> Map.ofSeq
        |> Map.ToKVP
        |> Seq.iter resp.Headers.Add
