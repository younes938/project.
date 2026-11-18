package com.example.dpsu.ui.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.example.dpsu.domain.UserRole
import com.example.dpsu.ui.TransportViewModel
import com.example.dpsu.ui.auth.RoleSelectionScreen
import com.example.dpsu.ui.checkpoint.CheckpointListScreen
import com.example.dpsu.ui.checkpoint.TransportDetailsScreen
import com.example.dpsu.ui.tyl.GreenTicketScreen
import com.example.dpsu.ui.tyl.RedTicketScreen
import com.example.dpsu.ui.tyl.TylHomeScreen

@Composable
fun AppNavigation(
    viewModel: TransportViewModel
) {
    val navController = rememberNavController()

    NavHost(
        navController = navController,
        startDestination = "role_selection"
    ) {
        composable("role_selection") {
            RoleSelectionScreen(
                onRoleSelected = { role ->
                    when (role) {
                        UserRole.VSH_TYL -> {
                            navController.navigate("tyl_home")
                        }

                        UserRole.PD,
                        UserRole.DMS,
                        UserRole.VSH_KORDON -> {
                            navController.navigate("checkpoint_list/${role.name}")
                        }
                    }
                }
            )
        }

        composable("tyl_home") {
            TylHomeScreen(
                onGreenClick = {
                    navController.navigate("green_ticket")
                },
                onRedClick = {
                    navController.navigate("red_ticket")
                },
                onListClick = {
                    navController.navigate("checkpoint_list/${UserRole.VSH_TYL.name}")
                },
                onBack = {
                    navController.popBackStack()
                }
            )
        }

        composable("green_ticket") {
            GreenTicketScreen(
                viewModel = viewModel,
                onSaved = {
                    navController.popBackStack()
                },
                onBack = {
                    navController.popBackStack()
                }
            )
        }

        composable("red_ticket") {
            RedTicketScreen(
                viewModel = viewModel,
                onSaved = {
                    navController.popBackStack()
                },
                onBack = {
                    navController.popBackStack()
                }
            )
        }

        composable(
            route = "checkpoint_list/{role}",
            arguments = listOf(
                navArgument("role") {
                    type = NavType.StringType
                }
            )
        ) { backStackEntry ->
            val role = UserRole.valueOf(
                backStackEntry.arguments?.getString("role") ?: UserRole.PD.name
            )

            CheckpointListScreen(
                role = role,
                viewModel = viewModel,
                onItemClick = { passId ->
                    navController.navigate("details/${role.name}/$passId")
                },
                onBack = {
                    navController.popBackStack()
                }
            )
        }

        composable(
            route = "details/{role}/{passId}",
            arguments = listOf(
                navArgument("role") {
                    type = NavType.StringType
                },
                navArgument("passId") {
                    type = NavType.StringType
                }
            )
        ) { backStackEntry ->
            val role = UserRole.valueOf(
                backStackEntry.arguments?.getString("role") ?: UserRole.PD.name
            )

            val passId = backStackEntry.arguments?.getString("passId") ?: ""

            TransportDetailsScreen(
                role = role,
                passId = passId,
                viewModel = viewModel,
                onBack = {
                    navController.popBackStack()
                },
                onActionDone = {
                    navController.popBackStack()
                }
            )
        }
    }
}
