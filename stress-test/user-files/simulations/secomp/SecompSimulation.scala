import scala.concurrent.duration._

import scala.util.Random

import io.gatling.core.Predef._
import io.gatling.http.Predef._

import java.util.UUID

class SecompSimulation
  extends Simulation {

  def uuid() = UUID.randomUUID.toString
  def randomValorTransacao() = BigDecimal(Random.between(0.01, 10000.0 + 1.0))
    .setScale(2, BigDecimal.RoundingMode.HALF_UP)

  val httpProtocol = http
    .baseUrl("http://localhost:5106")
    .userAgentHeader("Secomp - 2024")

  val transferencias = scenario("Solicitação de Transferência")
    .exec {s =>
      val clienteId = uuid()
      val valor = randomValorTransacao()
      val payload = s"""{"valor": ${valor}, "clienteId": "${clienteId}"}"""
      val session = s.setAll(Map("clienteId" -> clienteId, "payload" -> payload))
      session
    }
    .exec(
      http("transferências")
      .post("/").body(StringBody("#{payload}"))
      .header("content-type", "application/json")
      .check(status.is(201))
    )

  setUp(
    transferencias.inject(
      constantUsersPerSec(2).during(10.seconds),
      constantUsersPerSec(5).during(15.seconds),
      rampUsersPerSec(6).to(10).during(10.seconds)
    )
  ).protocols(httpProtocol)
}
