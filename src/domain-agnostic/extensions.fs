namespace DomainAgnostic

open System
open System.Collections.Generic
open System.Threading.Tasks


[<AutoOpen>]
module Disposible =

    type Disposable = IDisposable

    type IDisposable with

        static member DisposeOf(d: #IDisposable) =
            match box d with
            | null -> ()
            | _ -> d.Dispose()

        static member Defer func =
            { new IDisposable with
                member __.Dispose() = func() }


type Agent<'T> = MailboxProcessor<'T>

[<AutoOpen>]
module Agents =

    type MailboxProcessor<'T> with

        static member Receive(inbox: MailboxProcessor<'T>) = inbox.Receive()

        static member DefaultErrHandle _ prev = async.Return prev

        static member Supervised errHandle processor init token =
            let rec watch state inbox =
                async {
                    try
                        let! next = processor inbox state
                        return! watch next inbox
                    with e ->
                        try
                            let! next = errHandle e state
                            return! watch next inbox
                        with _ -> return! watch state inbox
                }
            new MailboxProcessor<_>(watch init, token)


[<AutoOpen>]
module OptionalMonad =

    type Option<'T> with

        static member Recover value o =
            match o with
            | Some value -> value
            | None -> value

        static member RecoverWith func o =
            match o with
            | Some value -> value
            | None -> func()

        static member OfOptional o =
            match o with
            | ValueSome value -> Some value
            | ValueNone -> None

        static member OfResult result =
            match result with
            | Ok value -> Some value
            | Error _ -> None

        static member OfNullable x =
            match box x with
            | null -> None
            | _ -> Some x

        static member ForceUnwrap err o =
            match o with
            | Some v -> v
            | None -> failwith err


[<RequireQualifiedAccess>]
module Seq =

    let Bind: ('a -> 'b seq) -> 'a seq -> 'b seq = Seq.collect

    let SkipFront n s =
        let l = Seq.length s
        Seq.skip (min l n) s

    let SkipBack n s =
        let l = Seq.length s
        Seq.truncate (l - n) s

    let OfOptional opt =
        match opt with
        | Some v -> Seq.singleton v
        | None -> Seq.empty

    let Appending elem sequence = Seq.singleton elem |> Seq.append sequence

    let NilIfEmpty(s: 'a seq) =
        if Seq.isEmpty s then None else Some s

    let Count predicate s =
        let count acc curr =
            match predicate curr with
            | true -> acc + 1
            | false -> acc
        s |> Seq.fold count 0

    let Contains predicate s = Count predicate s > 0

    let Partition predicate source =
        let map =
            source
            |> Seq.groupBy predicate
            |> Map.ofSeq

        let get flag =
            map
            |> Map.tryFind flag
            |> Option.defaultValue Seq.empty

        get true, get false

    let Crossproduct s1 s2 =
        seq {
            for e1 in s1 do
                for e2 in s2 do
                    yield e1, e2
        }


[<RequireQualifiedAccess>]
module List =

    let Bind: ('a -> 'b list) -> 'a list -> 'b list = List.collect

    let Prepending elem lst = elem :: lst

    let OfOptional opt =
        match opt with
        | Some v -> List.singleton v
        | None -> List.empty

    let Rest lst =
        match List.length lst with
        | 0 -> []
        | _ -> List.tail lst


[<RequireQualifiedAccess>]
module Map =

    let MapKV f m =
        m
        |> Map.toSeq
        |> Seq.map (fun (k, v) -> f k v)
        |> Map.ofSeq

    let MapKeys f m =
        let ff k v = (f k, v)
        MapKV ff m

    let MapValues f m =
        let ff k v = (k, f v)
        MapKV ff m

    let ToKVP m =
        m
        |> Map.toSeq
        |> Seq.map (fun (k, v) -> KeyValuePair(k, v))

    let OfKVP(kvp: KeyValuePair<'a, 'b> seq) =
        kvp
        |> Seq.map (fun kvp -> (kvp.Key, kvp.Value))
        |> Map.ofSeq


[<RequireQualifiedAccess>]
module Async =

    let SleepFor(span: TimeSpan) = int span.TotalMilliseconds |> Async.Sleep


[<AutoOpen>]
module ResultMonad =

    type Result<'T, 'E> with

        static member New func a =
            try
                func a |> Ok
            with e -> Error e

        static member ExnError msg = Exception(message = msg) |> Error

        static member OfOptional err o =
            match o with
            | Some v -> Ok v
            | None -> Error err

        static member Recover replacement result =
            match result with
            | Ok res -> res
            | Error _ -> replacement

        static member RecoverWith replace result =
            match result with
            | Ok res -> res
            | Error err -> replace err

        static member ForceUnwrap result =
            match result with
            | Ok res -> res
            | Error err -> raise err

        static member Discriminate s =
            let m (g, b) c =
                match c with
                | Ok v -> (Seq.Appending v g, b)
                | Error e -> (g, Seq.Appending e b)
            Seq.fold m (Seq.empty, Seq.empty) s


[<AutoOpen>]
module AsyncMonad =

    type Async with
        static member Return v = async.Return v

        static member New func a = async { return func a }

        static member Map func res = async {
                                         let! result = res
                                         return func result }

        static member Bind func res = async {
                                          let! result = res
                                          return! func result }

        static member BindTask (func: 'a -> Task<'b>) res =
            async {
                let! result = res
                return! result
                        |> func
                        |> Async.AwaitTask
            }

        static member StartAsPlainTask compute = compute |> Async.StartAsTask :> Task

        static member IgnoreTask task =
            task :> Task
            |> Async.AwaitTask
            |> Async.Ignore

        static member ParallelSeq a = async {
                                          let! res = a |> Async.Parallel
                                          return Seq.ofArray res }
