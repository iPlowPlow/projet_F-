namespace TimeOff

type Repository<'a> = {
    //rajouter seq for GetAll
    GetAll : unit -> 'a seq
    CreateTimeOff : TimeOffRequest -> 'a
}