package com.example.dpsu.utils

import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

object DateUtils {

    fun formatDateTime(timeMillis: Long?): String {
        if (timeMillis == null) return "—"

        val formatter = SimpleDateFormat(
            "dd.MM.yyyy HH:mm:ss",
            Locale.getDefault()
        )

        return formatter.format(Date(timeMillis))
    }
}
