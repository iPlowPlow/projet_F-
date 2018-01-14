namespace TimeOff

type Repository<'a> = {
    GetAll : unit -> 'a seq
    CreateTimeOff : TimeOffRequest -> 'a
    ValidateTimeOff : TimeOffRequest -> 'a
}