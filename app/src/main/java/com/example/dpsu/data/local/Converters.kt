package com.example.dpsu.data.local

import androidx.room.TypeConverter
import com.example.dpsu.domain.CurrentStage
import com.example.dpsu.domain.TicketType

class Converters {

    @TypeConverter
    fun fromTicketType(value: TicketType): String {
        return value.name
    }

    @TypeConverter
    fun toTicketType(value: String): TicketType {
        return TicketType.valueOf(value)
    }

    @TypeConverter
    fun fromCurrentStage(value: CurrentStage): String {
        return value.name
    }

    @TypeConverter
    fun toCurrentStage(value: String): CurrentStage {
        return CurrentStage.valueOf(value)
    }
}
