package com.example.dpsu.data.local

import androidx.room.Database
import androidx.room.RoomDatabase
import androidx.room.TypeConverters
import com.example.dpsu.data.local.dao.TransportPassDao
import com.example.dpsu.data.local.entity.TransportPassEntity

@Database(
    entities = [TransportPassEntity::class],
    version = 1,
    exportSchema = false
)
@TypeConverters(Converters::class)
abstract class AppDatabase : RoomDatabase() {

    abstract fun transportPassDao(): TransportPassDao
}
