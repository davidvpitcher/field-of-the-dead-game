<?php
// Check for POST variable<?php
// Check for POST variable
if (isset($_POST["code1234"])) {

    $unity = true;
    $login = true;

    require_once '../../keys/storage.php';

    // Query to fetch the leaderboard data
    $sql = "SELECT players.id, players.username, player_stats.net_gain_loss
            FROM players
            INNER JOIN player_stats ON players.id = player_stats.player_id
            ORDER BY player_stats.net_gain_loss ASC
            LIMIT 10";

    // Prepare and execute the query
    $stmt = $pdo->prepare($sql);
    $stmt->execute();

    // Check for successful query
    if ($stmt->rowCount() > 0) {
        $returnstring = "0"; // success code

        // Output the leaderboard data as a tab-separated string
        while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
            $returnstring .= "\t" . $row["id"] . "\t" . $row["username"] . "\t" . $row["net_gain_loss"];
        }

        echo $returnstring;
        exit();
    } else {
        echo "1"; // error code
        exit();
    }

    // Increment the net_gain_loss count for the player
    $sql = "INSERT INTO player_stats (player_id, net_gain_loss)
            VALUES ((SELECT id FROM players WHERE username = :username), -1)
            ON DUPLICATE KEY UPDATE net_gain_loss = net_gain_loss - 1;";
    
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username);

    if ($stmt->execute()) {
        echo "0";
    } else {
        echo "Error: Execution failed.";
    }
} else {
    echo "2"; // error code for missing POST variable
    exit();
}

echo "7: uh oh";
exit();
