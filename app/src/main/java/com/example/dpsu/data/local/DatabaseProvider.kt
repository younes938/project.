package com.example.dpsu.data.local

import android.content.Context
import androidx.room.Room

object DatabaseProvider {

    @Volatile
    private var INSTANCE: AppDatabase? = null

    fun getDatabase(context: Context): AppDatabase {
        return INSTANCE ?: synchronized(this) {
            val database = Room.databaseBuilder(
                context.applicationContext,
                AppDatabase::class.java,
                "dpsu_local_database"
            ).build()

            INSTANCE = database
            database
        }
    }
}
