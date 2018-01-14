namespace TimeOff.Repositories
open TimeOff

type UserRepository<'a> = {
    CreateTimeOff : TimeOffRequest -> 'a
    CancelTimeOffByEmployee : TimeOffRequest -> 'a
}