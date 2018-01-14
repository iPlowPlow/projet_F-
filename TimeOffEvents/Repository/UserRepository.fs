namespace TimeOff.Repository
open TimeOff

type UserRepository<'a> = {
    GetById : int -> 'a seq option
    CreateTimeOff : TimeOffRequest -> 'a option
    CancelTimeOffByEmployee : TimeOffRequest -> 'a option
}